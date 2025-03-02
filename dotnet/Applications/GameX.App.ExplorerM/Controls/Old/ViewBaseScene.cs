//using OpenStack;
//using OpenStack.Gfx;
//using OpenTK.Graphics.OpenGL;
//using System.ComponentModel;
//using System.Numerics;
//using System.Runtime.CompilerServices;

//namespace GameX.App.Explorer.Controls1
//{
//    //was:Renderer/GLSceneViewer
//    public abstract class GLSceneViewer : OpenGLView
//    {
//        public Scene Scene { get; private set; }
//        public Scene SkyboxScene { get; protected set; }

//        public bool ShowBaseGrid { get; set; } = true;
//        public bool ShowSkybox { get; set; } = true;

//        protected float SkyboxScale { get; set; } = 1.0f;
//        protected Vector3 SkyboxOrigin { get; set; } = Vector3.Zero;

//        bool ShowStaticOctree = false;
//        bool ShowDynamicOctree = false;
//        Frustum CullFrustum;

//        //ComboBox _renderModeComboBox;
//        ParticleGridRenderer BaseGrid;
//        Camera SkyboxCamera = new GLDebugCamera();
//        OctreeDebugRenderer<SceneNode> StaticOctreeRenderer;
//        OctreeDebugRenderer<SceneNode> DynamicOctreeRenderer;

//        protected GLSceneViewer(Frustum cullFrustum = null)
//        {
//            CullFrustum = cullFrustum;

//            InitializeControl();

//            //AddCheckBox("Show Grid", ShowBaseGrid, (v) => ShowBaseGrid = v);
//            //AddCheckBox("Show Static Octree", _showStaticOctree, (v) => _showStaticOctree = v);
//            //AddCheckBox("Show Dynamic Octree", _showDynamicOctree, (v) => _showDynamicOctree = v);
//            //AddCheckBox("Lock Cull Frustum", false, (v) => { _lockedCullFrustum = v ? Scene.MainCamera.ViewFrustum.Clone() : null; });

//            //Unloaded += (a, b) => { GLPaint -= OnPaint; };
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

//        public static readonly BindableProperty GfxProperty = BindableProperty.Create(nameof(Gfx), typeof(object), typeof(GLSceneViewer),
//            propertyChanged: (d, e, n) => (d as GLSceneViewer).OnProperty());

//        public IOpenGfx Gfx
//        {
//            get => GetValue(GfxProperty) as IOpenGfx;
//            set => SetValue(GfxProperty, value);
//        }

//        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(object), typeof(GLSceneViewer),
//            propertyChanged: (d, e, n) => (d as GLSceneViewer).OnProperty());

//        public object Source
//        {
//            get => GetValue(SourceProperty);
//            set => SetValue(SourceProperty, value);
//        }

//        void OnProperty()
//        {
//            if (Gfx == null || Source == null) return;

//            var gfx = Gfx as IOpenGLGfx;

//            Scene = new Scene(gfx, MeshBatchRenderer.Render);
//            BaseGrid = new ParticleGridRenderer(20, 5, gfx);

//            Camera.SetViewportSize((int)ActualWidth, (int)ActualHeight); //: HandleResize()
//            Camera.SetLocation(new Vector3(256));
//            Camera.LookAt(new Vector3(0));

//            LoadScene(Source);

//            if (Scene.AllNodes.Any())
//            {
//                var bbox = Scene.AllNodes.First().BoundingBox;
//                var location = new Vector3(bbox.Max.Z, 0, bbox.Max.Z) * 1.5f;

//                Camera.SetLocation(location);
//                Camera.LookAt(bbox.Center);
//            }

//            StaticOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.StaticOctree, Gfx as IOpenGLGfx, false);
//            DynamicOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.DynamicOctree, Gfx as IOpenGLGfx, true);

//            //if (_renderModeComboBox != null)
//            //{
//            //    var supportedRenderModes = Scene.AllNodes
//            //        .SelectMany(r => r.GetSupportedRenderModes())
//            //        .Distinct();
//            //    SetAvailableRenderModes(supportedRenderModes);
//            //}

//            GLPaint += OnPaint;
//        }

//        protected abstract void InitializeControl();

//        protected abstract void LoadScene(object source);

//        void OnPaint(object sender, RenderEventArgs e)
//        {
//            Scene.MainCamera = e.Camera;
//            Scene.Update(e.FrameTime);

//            if (ShowBaseGrid) BaseGrid.Render(e.Camera, RenderPass.Both);

//            if (ShowSkybox && SkyboxScene != null)
//            {
//                SkyboxCamera.CopyFrom(e.Camera);
//                SkyboxCamera.SetLocation(e.Camera.Location - SkyboxOrigin);
//                SkyboxCamera.SetScale(SkyboxScale);

//                SkyboxScene.MainCamera = SkyboxCamera;
//                SkyboxScene.Update(e.FrameTime);
//                SkyboxScene.RenderWithCamera(SkyboxCamera);

//                GL.Clear(ClearBufferMask.DepthBufferBit);
//            }

//            Scene.RenderWithCamera(e.Camera, CullFrustum);

//            if (ShowStaticOctree) StaticOctreeRenderer.Render(e.Camera, RenderPass.Both);
//            if (ShowDynamicOctree) DynamicOctreeRenderer.Render(e.Camera, RenderPass.Both);
//        }

//        protected void SetEnabledLayers(HashSet<string> layers)
//        {
//            Scene.SetEnabledLayers(layers);
//            StaticOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.StaticOctree, Gfx as IOpenGLGfx, false);
//        }

//        //protected void AddRenderModeSelectionControl()
//        //{
//        //    if (_renderModeComboBox == null)
//        //        _renderModeComboBox = AddSelection("Render Mode", (renderMode, _) =>
//        //        {
//        //            foreach (var node in Scene.AllNodes)
//        //                node.SetRenderMode(renderMode);

//        //            if (SkyboxScene != null)
//        //                foreach (var node in SkyboxScene.AllNodes)
//        //                    node.SetRenderMode(renderMode);
//        //        });
//        //}

//        //void SetAvailableRenderModes(IEnumerable<string> renderModes)
//        //{
//        //    _renderModeComboBox.Items.Clear();
//        //    if (renderModes.Any())
//        //    {
//        //        _renderModeComboBox.Enabled = true;
//        //        _renderModeComboBox.Items.Add("Default Render Mode");
//        //        _renderModeComboBox.Items.AddRange(renderModes.ToArray());
//        //        _renderModeComboBox.SelectedIndex = 0;
//        //    }
//        //    else
//        //    {
//        //        _renderModeComboBox.Items.Add("(no render modes available)");
//        //        _renderModeComboBox.SelectedIndex = 0;
//        //        _renderModeComboBox.Enabled = false;
//        //    }
//        //}
//    }
//}
