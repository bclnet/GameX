using OpenStack.Gfx;
using OpenStack.Gfx.Gl;
using OpenStack.Gfx.Renders;
using OpenStack.Gfx.Textures;
using OpenTK.Input;
using System.Windows;
using Key = OpenTK.Input.Key;
using ViewBase = OpenTK.Views.ViewBase;

namespace GameX.App.Explorer.Controls
{
    public class ViewOpenGL : GLViewerControl
    {
        int Id = 0;

        #region Binding

        protected object Obj;
        protected ViewBase View;

        public static readonly DependencyProperty GfxProperty = DependencyProperty.Register(nameof(Gfx), typeof(IOpenGfx), typeof(ViewOpenGL),
            new PropertyMetadata((d, e) => (d as ViewOpenGL).OnSourceChanged()));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(ViewOpenGL),
            new PropertyMetadata((d, e) => (d as ViewOpenGL).OnSourceChanged()));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(string), typeof(ViewOpenGL),
            new PropertyMetadata((d, e) => (d as ViewOpenGL).OnSourceChanged()));

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
            View = ViewBase.Create(Gfx as IOpenGLGfx, Source, Type);
            View.Start();
            if (Source is ITextureSelect z2) z2.Select(Id);
            //Camera.SetLocation(new Vector3(200));
            //Camera.LookAt(new Vector3(0));
        }

        #endregion

        #region Render

        protected override void SetViewport(int x, int y, int width, int height)
        {
            (int width, int height)? p = View?.GetViewport((width, height));
            if (p == null) base.SetViewport(x, y, width, height);
            else base.SetViewport(x, y, p.Value.width, p.Value.height);
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
            Views.FileExplorer.Current.OnInfoUpdated();
        }
        void MoveReset() { Id = 0; View.Level = 0..; OnSourceChanged(); }
        void MoveNext() { if (View.Level.Start.Value < 10) View.Level = new(View.Level.Start.Value + 1, View.Level.End); OnSourceChanged(); }
        void MovePrev() { if (View.Level.Start.Value > 0) View.Level = new(View.Level.Start.Value - 1, View.Level.End); OnSourceChanged(); }
        void Reset() { Id = 0; View.Level = 0..; OnSourceChanged(); }
        void Toggle() { View.ToggleValue = !View.ToggleValue; OnSourceChanged(); }

        #endregion
    }
}
