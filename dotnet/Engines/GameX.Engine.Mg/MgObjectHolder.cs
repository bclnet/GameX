namespace GameX.Engines.Mg;

/// <summary>
/// Mesh kept off the GPU: name, isStatic, mesh, materials, groups.
/// </summary>
public sealed class MgObjectHolder {
    public string Name;
    public bool IsStatic;
    public object Mesh;
    public object Materials;
    public object Groups;
}
