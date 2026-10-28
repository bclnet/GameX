using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenX;
using System.Runtime.InteropServices;
using XnaKeys = Microsoft.Xna.Framework.Input.Keyboard;

namespace GameX.Origin.Mg.UO;

/// <summary>
/// ClassicUO calls TextInputEXT.StartTextInput and reads KEY_DOWN and TEXT_INPUT on its window.
/// This window is MonoGame's SDL2. The SDL3 filter in the shared host never sees it.
/// </summary>
static class UoKeys {
    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    static extern void SDL_StartTextInput();

    public static void Attach(MgClientHost host) {
        SDL_StartTextInput();
        host.Window.KeyDown += (_, e) => GameX.Origin.UO.GameController.KeyDown(host.Scene, Map(e.Key), Mod());
        host.Window.KeyUp += (_, e) => GameX.Origin.UO.GameController.KeyUp(host.Scene, Map(e.Key), Mod());
        host.Window.TextInput += (_, e) => GameX.Origin.UO.GameController.TextInput(host.Scene, e.Character);
    }

    // LoginScene walks on SDL keycodes. Backspace, tab, return, and escape already match.
    static int Map(Keys key) => key switch {
        Keys.Up => 1073741906,
        Keys.Down => 1073741905,
        Keys.Right => 1073741903,
        Keys.Left => 1073741904,
        Keys.NumPad8 => 1073741920,
        Keys.NumPad2 => 1073741914,
        Keys.NumPad4 => 1073741916,
        Keys.NumPad6 => 1073741918,
        Keys.NumPad7 => 1073741919,
        Keys.NumPad9 => 1073741921,
        Keys.NumPad1 => 1073741913,
        Keys.NumPad3 => 1073741915,
        _ => (int)key,
    };

    /// <summary>
    /// SDL modifier bits LoginScene already tests. Shift is 0x0003, ctrl is 0x00C0, alt is 0x0300.
    /// </summary>
    static int Mod() {
        var keys = XnaKeys.GetState();
        var mod = 0;
        if (keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift)) mod |= 3;
        if (keys.IsKeyDown(Keys.LeftControl) || keys.IsKeyDown(Keys.RightControl)) mod |= 0x00C0;
        if (keys.IsKeyDown(Keys.LeftAlt) || keys.IsKeyDown(Keys.RightAlt)) mod |= 0x0300;
        return mod;
    }
}
