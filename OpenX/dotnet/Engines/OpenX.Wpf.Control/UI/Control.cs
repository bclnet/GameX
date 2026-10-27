using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using static OpenX.Wpf.Control.WindowsNative;

namespace OpenX.Wpf.Control;

public static class WindowsNative {
    [DllImport("User32.dll", EntryPoint = "SetParent")] public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("User32.dll", EntryPoint = "ShowWindow")] public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("User32.dll")] public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
}

public abstract class ShellControl : UserControl {
    public ShellControl() {
        AddChild(Host = new WindowsFormsHost());
        Host.Child = new System.Windows.Forms.MaskedTextBox("00/00/0000");
        Host.Loaded += OnLoaded;
        Host.Unloaded += OnUnloaded;
        Host.SizeChanged += OnSizeChanged;
    }

    #region Attach

    readonly WindowsFormsHost Host;
    Process Process;
    protected abstract object GetShellState();
    protected virtual string GetShellFile() => null;
    protected virtual string LogArgument(string log) => "";
    protected virtual string ExtraArguments(IntPtr parent) => "";
    protected virtual bool AttachParent => true;
    protected virtual bool KeepProcess => false;
    protected virtual void DeliverState(string token) { }

    // Only Unity sets KeepProcess, so one static slot is that player.
    static Process KeptProcess;
    static ShellControl KeptOwner;
    static IntPtr KeptHwnd;
    static System.Windows.Forms.NativeWindow Parking;

    protected static string FindRepoFile(string relative) {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "ENGINES.md")))
                return Path.Combine(dir.FullName, relative);
        return null;
    }

    // Matches ShellState.FromArgument. ST| fields are Base64Url so paths survive the command line.
    protected static string EncodeState(object state) {
        var text = state?.ToString();
        if (string.IsNullOrEmpty(text) || !text.StartsWith("ST|", StringComparison.Ordinal)) return text ?? "";
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return "ST." + b64;
    }

    static bool ProcessLive(System.Diagnostics.Process process) {
        try { return process != null && !process.HasExited; }
        catch { return false; }
    }

    static int LiveKeptId() {
        if (!ProcessLive(KeptProcess)) { KeptProcess = null; KeptHwnd = IntPtr.Zero; return -1; }
        return KeptProcess.Id;
    }

    static IntPtr PlayerHwnd(System.Diagnostics.Process process) {
        if (process != null)
            try {
                var hwnd = process.MainWindowHandle;
                if (hwnd != IntPtr.Zero) KeptHwnd = hwnd;
            }
            catch { }
        return KeptHwnd;
    }

    // Hidden top-level window. Windows destroys a child when its parent HWND is destroyed.
    static IntPtr ParkingHwnd() {
        if (Parking != null && Parking.Handle != IntPtr.Zero) return Parking.Handle;
        var window = new System.Windows.Forms.NativeWindow();
        window.CreateHandle(new System.Windows.Forms.CreateParams {
            Caption = "GameX.Unity.Park",
            Style = unchecked((int)0x80000000),
            ExStyle = 0x08000080,
            X = -32000,
            Y = -32000,
            Width = 1,
            Height = 1,
        });
        Parking = window;
        return window.Handle;
    }

    void Claim(System.Diagnostics.Process process) {
        if (KeptOwner != null && !ReferenceEquals(KeptOwner, this)) KeptOwner.Process = null;
        Process = process;
        KeptProcess = process;
        KeptOwner = this;
    }

    void ReuseKept(IntPtr host) {
        Claim(KeptProcess);
        var hwnd = PlayerHwnd(Process);
        if (hwnd != IntPtr.Zero) {
            SetParent(hwnd, host);
            var size = Host.RenderSize;
            if (size.Width > 1 && size.Height > 1)
                MoveWindow(hwnd, 0, 0, (int)size.Width, (int)size.Height, true);
        }
        DeliverState(EncodeState(GetShellState()));
    }

    void OnLoaded(object sender, RoutedEventArgs e) {
        var shellFile = GetShellFile();
        if (string.IsNullOrEmpty(shellFile)) return;
        var processFile = new FileInfo(shellFile);
        if (!processFile.Exists) { Console.WriteLine("Shell not found: " + shellFile); return; }
        try {
            var handle = Host.Handle;
            var processName = Path.GetFileNameWithoutExtension(processFile.Name);
            var keptId = LiveKeptId();
            var reuse = KeepProcess && keptId >= 0;
            foreach (var p in System.Diagnostics.Process.GetProcesses()) {
                try {
                    if (p.ProcessName != processName) continue;
                    if (p.Id == keptId) continue;
                    Console.WriteLine("Clean up extra processes, Process number: {0}", p.Id);
                    p.Kill();
                }
                catch (Exception) { }
            }
            if (reuse) { ReuseKept(handle); return; }
            Directory.CreateDirectory(processFile.DirectoryName);
            var log = Path.Combine(processFile.DirectoryName, "Shell.log");
            var token = EncodeState(GetShellState());
            Process = new Process();
            var si = Process.StartInfo;
            si.FileName = processFile.FullName;
            si.WorkingDirectory = processFile.DirectoryName;
            si.Arguments = string.Join(" ", new[] { LogArgument(log), ExtraArguments(handle), "--", token }.Where(s => !string.IsNullOrEmpty(s)));
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            Process.Start();
            try { Process.WaitForInputIdle(); }
            catch (InvalidOperationException) { }
            if (KeepProcess) Claim(Process);
            Thread.Sleep(100); // Wait for the handle
            if (!AttachParent) {
                // Unity parents itself from -parentHWND. Fit the child once it exists.
                for (var i = 0; i < 20 && Process.MainWindowHandle == IntPtr.Zero; i++) Thread.Sleep(50);
                var hwnd = PlayerHwnd(Process);
                var size = Host.RenderSize;
                if (hwnd != IntPtr.Zero && size.Width > 1 && size.Height > 1)
                    MoveWindow(hwnd, 0, 0, (int)size.Width, (int)size.Height, true);
                return;
            }
            SetParent(Process.MainWindowHandle, handle);
            _ = PlayerHwnd(Process);
            _ = ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    void OnUnloaded(object sender, RoutedEventArgs e) {
        try {
            if (Process == null) return;
            if (KeepProcess) {
                // Park before this host HWND is destroyed, or Windows destroys the player with it.
                if (ReferenceEquals(KeptOwner, this) && ProcessLive(Process)) {
                    var hwnd = PlayerHwnd(Process);
                    if (hwnd != IntPtr.Zero) SetParent(hwnd, ParkingHwnd());
                    KeptProcess = Process;
                    KeptOwner = null;
                }
                Process = null;
                return;
            }
            Process.CloseMainWindow();
            Thread.Sleep(1000);
            while (!Process.HasExited) Process.Kill();
        }
        catch (Exception) { }
    }

    void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        if (Process == null) return;
        var hwnd = Process.MainWindowHandle;
        if (hwnd == IntPtr.Zero && KeepProcess) hwnd = KeptHwnd;
        if (hwnd == IntPtr.Zero) return;
        var size = e.NewSize;
        MoveWindow(hwnd, 0, 0, (int)size.Width, (int)size.Height, true);
    }

    #endregion
}
