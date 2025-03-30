using GameX.Platforms.Godot;
using GameX.Platforms.Ogre;
using GameX.Platforms.OpenGL;
using GameX.Platforms.Sdl;
using GameX.Platforms.Stride;
using GameX.Platforms.Unity;
using OpenStack.Gfx;
using OpenStack.Gl;
using OpenStack.Godot;
using OpenStack.Ogre;
using OpenStack.Sdl;
using OpenStack.Stride;
using OpenStack.Unity;
using OpenStack.Wpf.Control;

namespace GameX.App.Explorer.Controls
{
    public class AppGodotControl : GodotControl
    {
        protected override Renderer CreateRenderer() => GodotRenderer.CreateRenderer(this, Gfx as IGodotGfx3d, Source, Type);
    }

    public class AppOgreControl : OgreControl
    {
        protected override Renderer CreateRenderer() => OgreRenderer.CreateRenderer(this, Gfx as IOgreGfx3d, Source, Type);
    }

    public class AppOpenGLControl : OpenGLControl
    {
        protected override Renderer CreateRenderer() => OpenGLRenderer.CreateRenderer(this, Gfx as IOpenGLGfx3d, Source, Type);
    }

    public class AppSdlControl : SdlControl
    {
        protected override Renderer CreateRenderer() => SdlRenderer.CreateRenderer(this, Gfx as ISdlGfx2d, Source, Type);
    }

    public class AppStrideControl : StrideControl
    {
        protected override Renderer CreateRenderer() => StrideRenderer.CreateRenderer(this, Gfx as IStrideGfx3d, Source, Type);
    }

    public class AppUnityControl : UnityControl
    {
        protected override Renderer CreateRenderer() => UnityRenderer.CreateRenderer(this, Gfx as IUnityGfx3d, Source, Type);
    }
}
