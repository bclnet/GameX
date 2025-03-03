using GameX.Platforms;
using OpenStack.Gfx;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using ViewBase = Godot.Views.ViewBase;

namespace GameX.App.Explorer.Controls
{
    public class ViewGodot : UserControl
    {
        public ViewGodot()
        {
            AddChild(Host = new WindowsFormsHost());
            Host.Child = new System.Windows.Forms.MaskedTextBox("00/00/0000");
            Host.Loaded += ViewGodot_Loaded;
            Host.Unloaded += ViewGodot_Unloaded;
            Host.SizeChanged += ViewGodot_SizeChanged;
        }

        #region Attach

        [DllImport("User32.dll", EntryPoint = "SetParent")] static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", EntryPoint = "ShowWindow")] static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("User32.dll")] static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        static string GodotFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Game2.exe");
        WindowsFormsHost Host;
        Process Process;

        void ViewGodot_Loaded(object sender, RoutedEventArgs e)
        {
            Process = null;
            var handle = Host.Handle;
            var processFile = new FileInfo(GodotFile);
            var processName = processFile.Name.Replace(".exe", ""); // Clean up extra processes beforehand
            foreach (var p in Process.GetProcesses().Where(p => p.ProcessName == processName))
            {
                Debug.WriteLine("Clean up extra processes, Process number: {0}", p.Id);
                p.Kill();
            }
            if (!processFile.Exists) return;
            Process = new Process();
            Process.StartInfo.FileName = GodotFile;
            Process.StartInfo.UseShellExecute = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            Process.WaitForInputIdle();
            Thread.Sleep(100); // Wait a minute for the handle
            SetParent(Process.MainWindowHandle, handle);
            ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
        }

        void ViewGodot_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Process != null)
                {
                    Process.CloseMainWindow();
                    Thread.Sleep(1000);
                    while (!Process.HasExited) Process.Kill();
                }
            }
            catch (Exception) { }
        }

        void ViewGodot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Process == null || Process.MainWindowHandle == IntPtr.Zero) return;
            var size = e.NewSize;
            MoveWindow(Process.MainWindowHandle, 0, 0, (int)size.Width, (int)size.Height, true);
        }

        #endregion

        #region Binding

        protected object Obj;
        protected ViewBase View;

        public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(ViewGodot),
            new PropertyMetadata((d, e) => (d as ViewGodot).OnSourceChanged()));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(ViewGodot),
            new PropertyMetadata((d, e) => (d as ViewGodot).OnSourceChanged()));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(ViewGodot),
            new PropertyMetadata((d, e) => (d as ViewGodot).OnSourceChanged()));

        public IOpenGfx Gfx
        {
            get => GetValue(GfxProperty) as IOpenGfx;
            set => SetValue(GfxProperty, value);
        }

        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public string Type
        {
            get => GetValue(TypeProperty) as string;
            set => SetValue(TypeProperty, value);
        }

        void OnSourceChanged()
        {
            if (Gfx == null || Source == null || Type == null) return;
            View = ViewBase.Create(Gfx as IGodotGfx, Source, Type);
        }

        #endregion
    }
}
