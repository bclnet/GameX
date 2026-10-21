namespace GameX.Engines.Ogre;

/// <summary>
/// Mesh kept off the GPU: name, isStatic, mesh, materials, groups.
/// </summary>
public sealed class OgreObjectHolder {
    public string Name;
    public bool IsStatic;
    public object Mesh;
    public object Materials;
    public object Groups;
}
