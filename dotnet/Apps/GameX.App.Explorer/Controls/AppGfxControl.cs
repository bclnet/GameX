using GameX.Engines.Godot;
using GameX.Engines.O3de;
using GameX.Engines.Ogre;
using GameX.Engines.OpenGL;
using GameX.Engines.Sdl;
using GameX.Engines.Stride;
using GameX.Engines.Unity;
using GameX.Engines.Unreal;
using OpenX;
using OpenX.Gfx;
using OpenX.Wpf.Control;

namespace GameX.App.Explorer.Controls;

public class AppOpenGLControl() : OpenGLControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => OpenGLRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppGodotControl() : GodotControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => GodotRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppO3deControl() : O3deControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => O3deRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppOgreControl() : OgreControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => OgreRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppSdlControl() : SdlControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => SdlRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppStrideControl() : StrideControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => StrideRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppUnityControl() : UnityControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => UnityRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}

public class AppUnrealControl() : UnrealControl(ShellState.Create) {
    protected override Renderer CreateRenderer() => UnrealRenderer.CreateRenderer(this, EngineX.Gfx, Source, Value, Type);
}
