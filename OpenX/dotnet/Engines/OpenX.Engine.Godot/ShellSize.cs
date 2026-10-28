namespace OpenX;

public static class ShellSize {
    static int _setW, _setH;

    public static void Apply() {
        var width = ShellPipe.Width;
        var height = ShellPipe.Height;
        if (width <= 1 || height <= 1 || (width == _setW && height == _setH)) return;
        Godot.DisplayServer.WindowSetSize(new Godot.Vector2I(width, height));
        _setW = width;
        _setH = height;
    }
}
