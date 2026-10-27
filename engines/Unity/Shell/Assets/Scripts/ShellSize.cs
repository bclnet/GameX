using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class ShellSize : MonoBehaviour {
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;
    static readonly IntPtr HWND_TOP = IntPtr.Zero;
    static readonly EnumProc Scan = OnScan;
    static IntPtr _exact, _child;
    static bool _scanningChild;

    IntPtr _parent, _window;
    int _width, _height, _frame;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {
        if (FindAnyObjectByType<ShellSize>() != null) return;
        var parent = ParentHwnd();
        if (parent == IntPtr.Zero) return;
        var go = new GameObject("ShellSize");
        DontDestroyOnLoad(go);
        go.AddComponent<ShellSize>()._parent = parent;
    }

    static IntPtr ParentHwnd() {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++) {
            if (args[i] != "-parentHWND") continue;
            return long.TryParse(args[i + 1], out var value) ? new IntPtr(value) : IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    // Parent client is device pixels. An outside MoveWindow does not resize the backbuffer.
    void Update() {
        if (!GetClientRect(_parent, out var rect)) return;
        var width = rect.right - rect.left;
        var height = rect.bottom - rect.top;
        if (width <= 1 || height <= 1) return;
        var hwnd = PlayerWindow();
        var screenDiffers = Screen.width != width || Screen.height != height;
        var frameDiffers = hwnd == IntPtr.Zero || !AtOrigin(hwnd, width, height);
        if (!screenDiffers && !frameDiffers) return;
        if (screenDiffers && (width != _width || height != _height || Time.frameCount - _frame >= 15)) {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            _width = width;
            _height = height;
            _frame = Time.frameCount;
        }
        if (hwnd != IntPtr.Zero && (frameDiffers || screenDiffers))
            SetWindowPos(hwnd, HWND_TOP, 0, 0, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    IntPtr PlayerWindow() {
        if (_window != IntPtr.Zero) return _window;
        var process = Process.GetCurrentProcess();
        var hwnd = process.MainWindowHandle;
        if (hwnd != IntPtr.Zero) return _window = hwnd;
        hwnd = Find(process.Id);
        if (hwnd != IntPtr.Zero) return _window = hwnd;
        hwnd = GetForegroundWindow();
        if (!Owned(hwnd, process.Id) || !IsPlayerClass(hwnd)) return IntPtr.Zero;
        return _window = hwnd;
    }

    IntPtr Find(int pid) {
        _exact = IntPtr.Zero;
        _child = IntPtr.Zero;
        var id = new IntPtr(pid);
        _scanningChild = false;
        EnumWindows(Scan, id);
        // EnumWindows misses the player once -parentHWND has made it a child.
        if (_exact == IntPtr.Zero) {
            _scanningChild = true;
            EnumChildWindows(_parent, Scan, id);
        }
        if (_exact != IntPtr.Zero) return _exact;
        return _child;
    }

    static bool OnScan(IntPtr hwnd, IntPtr id) {
        if (!Owned(hwnd, id.ToInt32()) || !IsWindowVisible(hwnd)) return true;
        if (_scanningChild && _child == IntPtr.Zero) _child = hwnd;
        if (!IsPlayerClass(hwnd)) return true;
        _exact = hwnd;
        return false;
    }

    static bool IsPlayerClass(IntPtr hwnd) {
        var name = new StringBuilder(32);
        return GetClassName(hwnd, name, name.Capacity) > 0 && name.ToString() == "UnityWndClass";
    }

    static bool Owned(IntPtr hwnd, int pid) {
        if (hwnd == IntPtr.Zero) return false;
        GetWindowThreadProcessId(hwnd, out var windowPid);
        return windowPid == (uint)pid;
    }

    bool AtOrigin(IntPtr hwnd, int width, int height) {
        if (!GetWindowRect(hwnd, out var window)) return false;
        if (window.right - window.left != width || window.bottom - window.top != height) return false;
        var origin = new POINT();
        return ClientToScreen(_parent, ref origin) && window.left == origin.x && window.top == origin.y;
    }

    delegate bool EnumProc(IntPtr hwnd, IntPtr lparam);

    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int x, y; }

    [DllImport("User32.dll")] static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("User32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("User32.dll")] static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
    [DllImport("User32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
    [DllImport("User32.dll")] static extern bool EnumWindows(EnumProc lpEnumFunc, IntPtr lParam);
    [DllImport("User32.dll")] static extern bool EnumChildWindows(IntPtr hWndParent, EnumProc lpEnumFunc, IntPtr lParam);
    [DllImport("User32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("User32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("User32.dll")] static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("User32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
}
