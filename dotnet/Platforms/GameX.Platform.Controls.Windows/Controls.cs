using GameX.Platforms;
using OpenStack.Gfx;
using OpenStack.Gfx.Gl;
using OpenStack.Gfx.Renders;
using OpenStack.Gfx.Textures;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using static GameX.App.Explorer.Controls.WindowsNative;
using GodotViewBase = Godot.Views.ViewBase;
using Key = OpenTK.Input.Key;
using OpenGLViewBase = OpenGL.Views.ViewBase;
//using StrideViewBase = Stride.Views.ViewBase;
using UnityViewBase = Unity.Views.ViewBase;

namespace GameX.App.Explorer.Controls;

#region WindowsNative

public static class WindowsNative
{
    [DllImport("User32.dll", EntryPoint = "SetParent")] public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("user32.dll", EntryPoint = "ShowWindow")] public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("User32.dll")] public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
}

#endregion

#region GodotControl

public class GodotControl : UserControl
{
    public GodotControl()
    {
        AddChild(Host = new WindowsFormsHost());
        Host.Child = new System.Windows.Forms.MaskedTextBox("00/00/0000");
        Host.Loaded += OnLoaded;
        Host.Unloaded += OnUnloaded;
        Host.SizeChanged += OnSizeChanged;
    }

    #region Attach

    static readonly string GodotFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Game2.exe");
    readonly WindowsFormsHost Host;
    Process Process;

    void OnLoaded(object sender, RoutedEventArgs e)
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
        _ = ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
    }

    void OnUnloaded(object sender, RoutedEventArgs e)
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

    void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Process == null || Process.MainWindowHandle == IntPtr.Zero) return;
        var size = e.NewSize;
        MoveWindow(Process.MainWindowHandle, 0, 0, (int)size.Width, (int)size.Height, true);
    }

    #endregion

    #region Binding

    protected object Obj;
    protected GodotViewBase View;

    public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(GodotControl), new PropertyMetadata((d, e) => (d as GodotControl).OnSourceChanged()));

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
        View = GodotViewBase.Create(this, Gfx as IGodotGfx, Source, Type);
    }

    #endregion
}

#endregion

#region OpenGLControl

public class OpenGLControl : GLViewerControl
{
    int Id = 0;

    #region Binding

    protected object Obj;
    protected OpenGLViewBase View;

    public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(OpenGLControl), new PropertyMetadata((d, e) => (d as OpenGLControl).OnSourceChanged()));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(OpenGLControl), new PropertyMetadata((d, e) => (d as OpenGLControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(OpenGLControl), new PropertyMetadata((d, e) => (d as OpenGLControl).OnSourceChanged()));

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
        View = OpenGLViewBase.CreateView(this, Gfx as IOpenGLGfx, Source, Type);
        View.Start();
        if (Source is ITextureSelect z2) z2.Select(Id);
        //Camera.SetLocation(new Vector3(200));
        //Camera.LookAt(new Vector3(0));
    }

    #endregion

    #region Render

    protected override void SetViewport(int x, int y, int width, int height)
    {
        (int width, int height) p = View?.GetViewport((width, height)) ?? (width, height);
        base.SetViewport(x, y, p.width, p.height);
    }

    protected override void Render(Camera camera, float frameTime)
        => View?.Render(camera, frameTime);

    public override void Tick(int? deltaTime = null)
    {
        base.Tick(deltaTime);
        View?.Update(DeltaTime);
        Render(Camera, 0f);
    }

    #endregion

    #region HandleInput

    static readonly Key[] Keys = [Key.Q, Key.W, Key.A, Key.Z, Key.Escape, Key.Space, Key.Tilde];
    readonly HashSet<Key> KeyDowns = [];

    protected override void HandleInput(MouseState mouseState, KeyboardState keyboardState)
    {
        if (View == null) return;
        foreach (var key in Keys)
            if (!KeyDowns.Contains(key) && keyboardState.IsKeyDown(key)) KeyDowns.Add(key);
        foreach (var key in KeyDowns)
            if (keyboardState.IsKeyUp(key))
            {
                KeyDowns.Remove(key);
                switch (key)
                {
                    case Key.W: Select(++Id); break;
                    case Key.Q: Select(--Id); break;
                    case Key.A: MovePrev(); break;
                    case Key.Z: MoveNext(); ; break;
                    case Key.Escape: Reset(); break;
                    case Key.Space: MoveReset(); break;
                    case Key.Tilde: Toggle(); break;
                }
            }
    }

    void Select(int id)
    {
        if (Obj is ITextureSelect z2) z2.Select(id);
        OnSourceChanged();
        //Views.FileExplorer.Current.OnInfoUpdated();
    }
    void MoveReset() { Id = 0; View.Level = 0..; OnSourceChanged(); }
    void MoveNext() { if (View.Level.Start.Value < 10) View.Level = new(View.Level.Start.Value + 1, View.Level.End); OnSourceChanged(); }
    void MovePrev() { if (View.Level.Start.Value > 0) View.Level = new(View.Level.Start.Value - 1, View.Level.End); OnSourceChanged(); }
    void Reset() { Id = 0; View.Level = 0..; OnSourceChanged(); }
    void Toggle() { View.ToggleValue = !View.ToggleValue; OnSourceChanged(); }

    #endregion
}

#endregion

#region StrideControl
#if false

using Stride.CommunityToolkit.Engine;
using Stride.Core.Presentation.Controls;
using Stride.Editor.Engine;
using System.Threading.Tasks;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering.ProceduralModels;

public class StrideControl : UserControl
{
    #region Embedding

    readonly TaskCompletionSource<bool> GameStartedTaskSource = new();
    Thread GameThread;
    IntPtr WindowHandle;

    public StrideControl()
    {
        GameThread = new Thread(SafeAction.Wrap(GameRunThread))
        {
            IsBackground = true,
            Name = "Game Thread"
        };
        GameThread.SetApartmentState(ApartmentState.STA);
        Loaded += async (sender, args) => await StartGame();
    }

    async Task StartGame()
    {
        GameThread.Start();
        await GameStartedTaskSource.Task;
        Content = new GameEngineHost(WindowHandle);
    }

    void GameRunThread()
    {
        // Create the form from this thread
        var form = new EmbeddedGameForm()
        {
            TopLevel = false,
            Visible = false
        };
        WindowHandle = form.Handle;
        var context = new GameContextWinforms(form);
        GameStartedTaskSource.SetResult(true);
        var game = new Game();
        game.Run(context, (Scene scene) =>
        {
            game.Window.IsBorderLess = true;
            game.SetupBase();

            var entity1 = new Entity("Name", new Vector3(1f, 0.5f, 3f))
                {
                    new ModelComponent(new CubeProceduralModel().Generate(game.Services))
                };
            scene.Entities.Add(entity1);

            //{
            //    // Create an entity and add it to the scene.
            //    var entity = new Entity();
            //    scene.Entities.Add(entity);
            //    // Create a model and assign it to the model component.
            //    var model = new Model();
            //    entity.GetOrCreate<ModelComponent>().Model = model;
            //    // Add one or more meshes using geometric primitives (eg spheres or cubes).
            //    var meshDraw = GeometricPrimitive.Sphere.New(game.GraphicsDevice).ToMeshDraw();
            //    var mesh = new Mesh { Draw = meshDraw };
            //    model.Meshes.Add(mesh);
            //}

            //{
            //    var entity = new Entity();
            //    scene.Entities.Add(entity);
            //    var model = new Model();
            //    entity.GetOrCreate<ModelComponent>().Model = model;
            //    var vertices = new VertexPositionTexture[3];
            //    vertices[0].Position = new Vector3(0f, 0f, 1f);
            //    vertices[1].Position = new Vector3(0f, 1f, 0f);
            //    vertices[2].Position = new Vector3(0f, 1f, 1f);
            //    var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(game.GraphicsDevice, vertices, GraphicsResourceUsage.Dynamic);
            //    int[] indices = { 0, 2, 1 };
            //    var indexBuffer = Stride.Graphics.Buffer.Index.New(game.GraphicsDevice, indices);

            //    var customMesh = new Mesh
            //    {
            //        Draw = new MeshDraw
            //        {
            //            /* Vertex buffer and index buffer setup */
            //            PrimitiveType = PrimitiveType.TriangleList,
            //            DrawCount = indices.Length,
            //            IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Length),
            //            VertexBuffers = [new VertexBufferBinding(vertexBuffer, VertexPositionTexture.Layout, vertexBuffer.ElementCount)],
            //        }
            //    };
            //    // add the mesh to the model
            //    model.Meshes.Add(customMesh);
            //}
        });
    }

    #endregion

    #region Binding

    protected object Obj;
    protected StrideViewBase View;

    public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(StrideControl), new PropertyMetadata((d, e) => (d as StrideControl).OnSourceChanged()));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(StrideControl), new PropertyMetadata((d, e) => (d as StrideControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(StrideControl), new PropertyMetadata((d, e) => (d as StrideControl).OnSourceChanged()));

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
        View = StrideViewBase.Create(this, Gfx as IStrideGfx, Source, Type);
        View.Start();
    }

    #endregion

    #region BS

    //    void Start(Scene rootScene)
    //    {
    //        game.SetupBase3DScene();
    //        game.AddSkybox();

    //        AddMesh(game.GraphicsDevice, rootScene, Vector3.Zero, GiveMeATriangle);
    //        AddMesh(game.GraphicsDevice, rootScene, Vector3.UnitX * 2, GiveMeAPlane);
    //    }

    //    void Update(Scene rootScene, GameTime gameTime)
    //    {
    //        var segments = (int)((Math.Cos(gameTime.Total.TotalMilliseconds / 500) + 1) / 2 * 47) + 3;
    //        circleEntity?.Remove();
    //        circleEntity = AddMesh(game.GraphicsDevice, rootScene, Vector3.UnitX * -2, b => GiveMeACircle(b, segments));
    //    }

    //    void GiveMeATriangle(MeshBuilder meshBuilder)
    //    {
    //        meshBuilder.WithIndexType(IndexingType.Int16);
    //        meshBuilder.WithPrimitiveType(PrimitiveType.TriangleList);

    //        var position = meshBuilder.WithPosition<Vector3>();
    //        var color = meshBuilder.WithColor<Color>();

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(0, 0, 0));
    //        meshBuilder.SetElement(color, Color.Red);

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(1, 0, 0));
    //        meshBuilder.SetElement(color, Color.Green);

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(.5f, 1, 0));
    //        meshBuilder.SetElement(color, Color.Blue);

    //        meshBuilder.AddIndex(0);
    //        meshBuilder.AddIndex(2);
    //        meshBuilder.AddIndex(1);
    //    }

    //    void GiveMeAPlane(MeshBuilder meshBuilder)
    //    {
    //        meshBuilder.WithIndexType(IndexingType.Int16);
    //        meshBuilder.WithPrimitiveType(PrimitiveType.TriangleList);

    //        var position = meshBuilder.WithPosition<Vector3>();
    //        var color = meshBuilder.WithColor<Color>();

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(0, 0, 0));
    //        meshBuilder.SetElement(color, Color.Red);

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(0, 1, 0));
    //        meshBuilder.SetElement(color, Color.Green);

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(1, 1, 0));
    //        meshBuilder.SetElement(color, Color.Blue);

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(1, 0, 0));
    //        meshBuilder.SetElement(color, Color.Yellow);

    //        meshBuilder.AddIndex(0);
    //        meshBuilder.AddIndex(1);
    //        meshBuilder.AddIndex(2);

    //        meshBuilder.AddIndex(0);
    //        meshBuilder.AddIndex(2);
    //        meshBuilder.AddIndex(3);
    //    }

    //    void GiveMeACircle(MeshBuilder meshBuilder, int segments)
    //    {
    //        meshBuilder.WithIndexType(IndexingType.Int16);
    //        meshBuilder.WithPrimitiveType(PrimitiveType.TriangleList);

    //        var position = meshBuilder.WithPosition<Vector3>();
    //        var color = meshBuilder.WithColor<Color4>();

    //        for (var i = 0; i < segments; i++)
    //        {
    //            var x = (float)Math.Sin(Math.Tau / segments * i) / 2;
    //            var y = (float)Math.Cos(Math.Tau / segments * i) / 2;
    //            var hsl = new ColorHSV(360f / segments * i, 1, 1, 1).ToColor();

    //            meshBuilder.AddVertex();
    //            meshBuilder.SetElement(position, new Vector3(x + .5f, y + .5f, 0));
    //            meshBuilder.SetElement(color, hsl);
    //        }

    //        meshBuilder.AddVertex();
    //        meshBuilder.SetElement(position, new Vector3(.5f, .5f, 0));
    //        meshBuilder.SetElement(color, Color.Black.ToColor4());

    //        for (var i = 0; i < segments; i++)
    //        {
    //            meshBuilder.AddIndex(segments);
    //            meshBuilder.AddIndex(i);
    //            meshBuilder.AddIndex((i + 1) % segments);
    //        }
    //    }

    //    Entity AddMesh(GraphicsDevice graphicsDevice, Scene rootScene, Vector3 position, Action<MeshBuilder> build)
    //    {
    //        using var meshBuilder = new MeshBuilder();
    //        build(meshBuilder);

    //        var entity = new Entity { Scene = rootScene, Transform = { Position = position } };
    //        var model = new Model
    //{
    //    new MaterialInstance {
    //        Material = Material.New(graphicsDevice, new MaterialDescriptor {
    //            Attributes = new MaterialAttributes {
    //                DiffuseModel = new MaterialDiffuseLambertModelFeature(),
    //                Diffuse = new MaterialDiffuseMapFeature {
    //                    DiffuseMap = new ComputeVertexStreamColor()
    //                },
    //            }
    //        })
    //    },
    //    new Mesh {
    //        Draw = meshBuilder.ToMeshDraw(graphicsDevice),
    //        MaterialIndex = 0
    //    }
    //};
    //        entity.Add(new ModelComponent { Model = model });
    //        return entity;
    //    }

    #endregion
}

#endif
#endregion

#region UnityControl

public class UnityControl : UserControl
{
    public UnityControl()
    {
        AddChild(Host = new WindowsFormsHost());
        Host.Child = new System.Windows.Forms.MaskedTextBox("00/00/0000");
        Host.Loaded += OnLoaded;
        Host.Unloaded += OnUnloaded;
        Host.SizeChanged += OnSizeChanged;
    }

    #region Attach

    static readonly string GodotFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Game2.exe");
    readonly WindowsFormsHost Host;
    Process Process;

    void OnLoaded(object sender, RoutedEventArgs e)
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
        _ = ShowWindow(Process.MainWindowHandle, (int)ProcessWindowStyle.Maximized);
    }

    void OnUnloaded(object sender, RoutedEventArgs e)
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

    void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Process == null || Process.MainWindowHandle == IntPtr.Zero) return;
        var size = e.NewSize;
        MoveWindow(Process.MainWindowHandle, 0, 0, (int)size.Width, (int)size.Height, true);
    }

    #endregion

    #region Binding

    protected object Obj;
    protected UnityViewBase View;

    public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(UnityControl), new PropertyMetadata((d, e) => (d as UnityControl).OnSourceChanged()));

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
        View = UnityViewBase.Create(this, Gfx as IUnityGfx, Source, Type);
    }

    #endregion
}

#endregion
