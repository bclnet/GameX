using Stride.Engine;

namespace GameX.Engines.Stride;

/// <summary>
/// Mesh kept off the GPU. Stride's builder dictionary returns Entity, so this rides on that entity.
/// </summary>
public sealed class StrideObjectHolder : EntityComponent {
    public string Name;
    public bool IsStatic;
    public object Mesh;
    public object Materials;
    public object Groups;
}
