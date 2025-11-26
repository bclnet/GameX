using GameX.Platforms.Godot;
using GameX.Platforms.O3de;
using GameX.Platforms.Ogre;
using GameX.Platforms.OpenGL;
using GameX.Platforms.Sdl;
using GameX.Platforms.Stride;
using GameX.Platforms.Unity;
using GameX.Platforms.Unreal;
using OpenStack.Gfx;
using OpenStack.Wpf.Control;

namespace GameX.App.Explorer.Controls;

public class AppOpenGLControl : OpenGLControl {
    protected override Renderer CreateRenderer() => OpenGLRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppGodotControl : GodotControl {
    protected override Renderer CreateRenderer() => GodotRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppO3deControl : O3deControl {
    protected override Renderer CreateRenderer() => O3deRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppOgreControl : OgreControl {
    protected override Renderer CreateRenderer() => OgreRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppSdlControl : SdlControl {
    protected override Renderer CreateRenderer() => SdlRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppStrideControl : StrideControl {
    protected override Renderer CreateRenderer() => StrideRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppUnityControl : UnityControl {
    protected override Renderer CreateRenderer() => UnityRenderer.CreateRenderer(this, Gfx, Source, Type);
}

public class AppUnrealControl : UnrealControl {
    protected override Renderer CreateRenderer() => UnrealRenderer.CreateRenderer(this, Gfx, Source, Type);
}
