using GameX.Platforms.Godot;
using GameX.Platforms.O3de;
using GameX.Platforms.Ogre;
using GameX.Platforms.OpenGL;
using GameX.Platforms.Sdl;
using GameX.Platforms.Stride;
using GameX.Platforms.Unity;
using GameX.Platforms.Unreal;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Wpf.Control;
using static OpenStack.Gfx.GfX;

namespace GameX.App.Explorer.Controls
{
    public class AppOpenGLControl : OpenGLControl
    {
        protected override Renderer CreateRenderer() => OpenGLRenderer.CreateRenderer(this, Gfx[XModel] as OpenGLGfxModel, Source, Type);
    }

    public class AppGodotControl : GodotControl
    {
        protected override Renderer CreateRenderer() => GodotRenderer.CreateRenderer(this, Gfx[XModel] as GodotGfxModel, Source, Type);
    }

    public class AppO3deControl : O3deControl
    {
        protected override Renderer CreateRenderer() => O3deRenderer.CreateRenderer(this, Gfx[XModel] as O3deGfxModel, Source, Type);
    }

    public class AppOgreControl : OgreControl
    {
        protected override Renderer CreateRenderer() => OgreRenderer.CreateRenderer(this, Gfx[XModel] as OgreGfxModel, Source, Type);
    }

    public class AppSdlControl : SdlControl
    {
        protected override Renderer CreateRenderer() => SdlRenderer.CreateRenderer(this, Gfx[XSprite2D] as SdlGfxSprite2D, Source, Type);
    }

    public class AppStrideControl : StrideControl
    {
        protected override Renderer CreateRenderer() => StrideRenderer.CreateRenderer(this, Gfx[XModel] as StrideGfxModel, Source, Type);
    }

    public class AppUnityControl : UnityControl
    {
        protected override Renderer CreateRenderer() => UnityRenderer.CreateRenderer(this, Gfx[XModel] as UnityGfxModel, Source, Type);
    }

    public class AppUnrealControl : UnrealControl
    {
        protected override Renderer CreateRenderer() => UnrealRenderer.CreateRenderer(this, Gfx[XModel] as UnrealGfxModel, Source, Type);

        void X()
        {
            //OpenStack.Gfx
        }
    }
}
