using Microsoft.Xna.Framework.Input;
using OpenX;
using UoPointer = GameX.Origin.UO.Mouse;
using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// Window pointer for GameX.Origin.UO.Mouse. Origin.UO is netstandard2.1 and cannot reference MonoGame.
/// GetState is that window.
/// </summary>
static class UoMouse {
    public static void Attach(MgClientHost host) => UoPointer.Read = () => Read(host);

    static UoPointer.Sample Read(MgClientHost host) {
        var mouse = XnaMouse.GetState(host.Window);
        var win = host.Window.ClientBounds;
        return new UoPointer.Sample {
            X = mouse.X,
            Y = mouse.Y,
            WinW = win.Width,
            WinH = win.Height,
            BufW = host.GraphicManager.PreferredBackBufferWidth,
            BufH = host.GraphicManager.PreferredBackBufferHeight,
            Wheel = mouse.ScrollWheelValue,
            Left = mouse.LeftButton == ButtonState.Pressed,
            Middle = mouse.MiddleButton == ButtonState.Pressed,
            Right = mouse.RightButton == ButtonState.Pressed,
            X1 = mouse.XButton1 == ButtonState.Pressed,
            X2 = mouse.XButton2 == ButtonState.Pressed,
            InWindow = win.Width > 0 && win.Height > 0 && mouse.X >= 0 && mouse.Y >= 0 && mouse.X < win.Width && mouse.Y < win.Height,
        };
    }
}
