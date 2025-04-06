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
using static OpenStack.Gfx.GFX;

namespace GameX.App.Explorer.Controls
{
    public class AppOpenGLControl : OpenGLControl
    {
        protected override Renderer CreateRenderer() => OpenGLRenderer.CreateRenderer(this, Gfx[X3dModel] as OpenGLGfx3dModel, Source, Type);
    }

    public class AppGodotControl : GodotControl
    {
        protected override Renderer CreateRenderer() => GodotRenderer.CreateRenderer(this, Gfx[X3dModel] as GodotGfx3dModel, Source, Type);
    }

    public class AppOgreControl : OgreControl
    {
        protected override Renderer CreateRenderer() => OgreRenderer.CreateRenderer(this, Gfx[X3dModel] as OgreGfx3dModel, Source, Type);
    }

    public class AppSdlControl : SdlControl
    {
        protected override Renderer CreateRenderer() => SdlRenderer.CreateRenderer(this, Gfx[X2dSprite] as SdlGfx2dSprite, Source, Type);
    }

    public class AppStrideControl : StrideControl
    {
        protected override Renderer CreateRenderer() => StrideRenderer.CreateRenderer(this, Gfx[X3dModel] as StrideGfx3dModel, Source, Type);
    }

    public class AppUnityControl : UnityControl
    {
        protected override Renderer CreateRenderer() => UnityRenderer.CreateRenderer(this, Gfx[X3dModel] as UnityGfx3dModel, Source, Type);
    }
}
