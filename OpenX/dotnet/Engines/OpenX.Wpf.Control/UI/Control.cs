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

    protected static string FindRepoFile(string relative) {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "ENGINES.md")))
                return Path.Combine(dir.FullName, relative);
        return null;
    }

    // Matches ShellState.FromArgument. ST| fields are Base64Url so paths survive the command line.
    static string EncodeState(object state) {
        var text = state?.ToString();
        if (string.IsNullOrEmpty(text) || !text.StartsWith("ST|", StringComparison.Ordinal)) return text ?? "";
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return "ST." + b64;
    }

    void OnLoaded(object sender, RoutedEventArgs e) {
        Process = null;
        var shellFile = GetShellFile();
        if (string.IsNullOrEmpty(shellFile)) return;
        var processFile = new FileInfo(shellFile);
        if (!processFile.Exists) return;
        try {
            var handle = Host.Handle;
            var processName = Path.GetFileNameWithoutExtension(processFile.Name);
            foreach (var p in System.Diagnostics.Process.GetProcesses().Where(p => p.ProcessName == processName)) {
                Console.WriteLine("Clean up extra processes, Process number: {0}", p.Id);
                try { p.Kill(); }
                catch (Exception) { }
            }
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
            Thread.Sleep(100); // Wait a minute for the handle
            if (!AttachParent) return;
            SetParent(Process.MainWindowHandle, handle);
            _ = ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    void OnUnloaded(object sender, RoutedEventArgs e) {
        try {
            if (Process != null) {
                Process.CloseMainWindow();
                Thread.Sleep(1000);
                while (!Process.HasExited) Process.Kill();
            }
        }
        catch (Exception) { }
    }

    void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        if (Process == null || Process.MainWindowHandle == IntPtr.Zero) return;
        var size = e.NewSize;
        MoveWindow(Process.MainWindowHandle, 0, 0, (int)size.Width, (int)size.Height, true);
    }

    #endregion
}
