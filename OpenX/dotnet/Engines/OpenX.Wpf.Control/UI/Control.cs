using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int left, top, right, bottom; }

    [DllImport("User32.dll", EntryPoint = "SetParent")] public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("User32.dll", EntryPoint = "ShowWindow")] public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("User32.dll")] public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
    [DllImport("User32.dll")] public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
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
    // One pipe and one parked process per name. Unity and Godot must not share a slot.
    protected virtual string PipeName => null;

    sealed class ShellLink {
        public Process Process;
        public ShellControl Owner;
        public IntPtr Hwnd;
        public NamedPipeServerStream Pipe;
        public string Item, Size;
        public readonly object Gate = new();
    }

    static readonly Dictionary<string, ShellLink> Links = new();
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

    ShellLink Link() {
        var name = PipeName;
        if (string.IsNullOrEmpty(name)) return null;
        if (!Links.TryGetValue(name, out var link)) Links[name] = link = new ShellLink();
        return link;
    }

    static bool ProcessLive(System.Diagnostics.Process process) {
        try { return process != null && !process.HasExited; }
        catch { return false; }
    }

    static int LiveId(ShellLink link) {
        if (link == null || !ProcessLive(link.Process)) {
            if (link != null) { link.Process = null; link.Hwnd = IntPtr.Zero; }
            return -1;
        }
        return link.Process.Id;
    }

    static IntPtr PlayerHwnd(ShellLink link) {
        if (link?.Process != null)
            try {
                var hwnd = link.Process.MainWindowHandle;
                if (hwnd != IntPtr.Zero) link.Hwnd = hwnd;
            }
            catch { }
        return link?.Hwnd ?? IntPtr.Zero;
    }

    // Hidden top-level window. Windows destroys a child when its parent HWND is destroyed.
    static IntPtr ParkingHwnd() {
        if (Parking != null && Parking.Handle != IntPtr.Zero) return Parking.Handle;
        var window = new System.Windows.Forms.NativeWindow();
        window.CreateHandle(new System.Windows.Forms.CreateParams {
            Caption = "GameX.Shell.Park",
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

    void Claim(ShellLink link, System.Diagnostics.Process process) {
        if (link.Owner != null && !ReferenceEquals(link.Owner, this)) link.Owner.Process = null;
        Process = process;
        link.Process = process;
        link.Owner = this;
    }

    protected void DeliverState(string token) {
        try {
            var link = Link();
            if (link == null || string.IsNullOrEmpty(token)) return;
            lock (link.Gate) {
                link.Item = token;
                Ensure(link);
                Flush(link);
            }
        }
        catch { }
    }

    void DeliverSize() {
        try {
            var link = Link();
            if (link == null) return;
            var host = Host.Handle;
            if (host == IntPtr.Zero || !GetClientRect(host, out var rect)) return;
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;
            if (width <= 1 || height <= 1) return;
            lock (link.Gate) {
                link.Size = "SZ|" + width + "|" + height;
                Ensure(link);
                Flush(link);
            }
        }
        catch { }
    }

    void Ensure(ShellLink link) {
        if (link.Pipe != null) return;
        var pipe = new NamedPipeServerStream(PipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        link.Pipe = pipe;
        pipe.BeginWaitForConnection(ar => {
            try {
                pipe.EndWaitForConnection(ar);
                lock (link.Gate) Flush(link);
            }
            catch { }
        }, null);
    }

    static void Flush(ShellLink link) {
        var pipe = link.Pipe;
        if (pipe == null || !pipe.IsConnected) return;
        Write(pipe, link.Item);
        Write(pipe, link.Size);
    }

    static void Write(NamedPipeServerStream pipe, string line) {
        if (string.IsNullOrEmpty(line)) return;
        var bytes = Encoding.UTF8.GetBytes(line + "\n");
        pipe.Write(bytes, 0, bytes.Length);
        pipe.Flush();
    }

    void ReuseKept(ShellLink link, IntPtr host) {
        Claim(link, link.Process);
        var hwnd = PlayerHwnd(link);
        if (hwnd != IntPtr.Zero) SetParent(hwnd, host);
        DeliverState(EncodeState(GetShellState()));
        DeliverSize();
    }

    void OnLoaded(object sender, RoutedEventArgs e) {
        var shellFile = GetShellFile();
        if (string.IsNullOrEmpty(shellFile)) return;
        var processFile = new FileInfo(shellFile);
        if (!processFile.Exists) { Console.WriteLine("Shell not found: " + shellFile); return; }
        try {
            var handle = Host.Handle;
            var processName = Path.GetFileNameWithoutExtension(processFile.Name);
            var link = KeepProcess ? Link() : null;
            var keptId = LiveId(link);
            var reuse = link != null && keptId >= 0;
            foreach (var p in System.Diagnostics.Process.GetProcesses()) {
                try {
                    if (p.ProcessName != processName) continue;
                    if (p.Id == keptId) continue;
                    Console.WriteLine("Clean up extra processes, Process number: {0}", p.Id);
                    p.Kill();
                }
                catch (Exception) { }
            }
            if (reuse) { ReuseKept(link, handle); return; }
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
            if (link != null) Claim(link, Process);
            Thread.Sleep(100); // Wait for the handle
            if (!AttachParent) {
                // Unity parents itself from -parentHWND. The player sizes itself from the pipe.
                for (var i = 0; i < 20 && Process.MainWindowHandle == IntPtr.Zero; i++) Thread.Sleep(50);
                _ = PlayerHwnd(link);
                if (KeepProcess) DeliverSize();
                return;
            }
            SetParent(Process.MainWindowHandle, handle);
            _ = PlayerHwnd(link);
            if (KeepProcess) DeliverSize();
            else _ = ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    void OnUnloaded(object sender, RoutedEventArgs e) {
        try {
            if (Process == null) return;
            if (KeepProcess) {
                // Park before this host HWND is destroyed, or Windows destroys the player with it.
                var link = Link();
                if (link != null && ReferenceEquals(link.Owner, this) && ProcessLive(Process)) {
                    var hwnd = PlayerHwnd(link);
                    if (hwnd != IntPtr.Zero) SetParent(hwnd, ParkingHwnd());
                    link.Owner = null;
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
        if (KeepProcess) { DeliverSize(); return; }
        var hwnd = Process.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;
        var size = e.NewSize;
        MoveWindow(hwnd, 0, 0, (int)size.Width, (int)size.Height, true);
    }

    #endregion
}
