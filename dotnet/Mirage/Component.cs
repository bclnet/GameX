using System.Drawing;
using System.Numerics;

namespace Mirage;

/// <summary>
/// Component - behaviour or data attached to a Node. Unity's MonoBehaviour/Component.
/// Ready() runs once, on the first frame after the component enters the tree; Process() runs every frame while enabled.
/// </summary>
public abstract class Component {
    public Node Node { get; internal set; }
    public bool Enabled = true;
    public bool IsReady { get; internal set; }
    public SceneTree Tree => Node?.Tree;

    /// <summary>
    /// The name used in scene text - the type name.
    /// </summary>
    public string Kind => GetType().Name;

    public virtual void Ready() { }
    public virtual void Process(float delta) { }

    /// <summary>
    /// Writes this component's properties for scene text. Override and call w.Prop / w.Flag.
    /// </summary>
    public virtual void WriteProps(SceneText.Props w) { }

    /// <summary>
    /// Reads one property from scene text. Return false for an unknown key.
    /// </summary>
    public virtual bool ReadProp(string key, string value, ResourceSet resources) => false;

    public virtual Component Clone() {
        var c = (Component)MemberwiseClone();
        c.Node = null;
        c.IsReady = false;
        return c;
    }

    public override string ToString() => SceneText.Write(this);
}

#region Rendering

/// <summary>
/// MeshRenderer - draws a Mesh with a Material. Also stands in for Unity's MeshFilter.
/// </summary>
public class MeshRenderer : Component {
    public Mesh Mesh;
    public Material Material;
    public override void WriteProps(SceneText.Props w) { w.Prop("mesh", Mesh?.Name); w.Prop("material", Material?.Name); }
    public override bool ReadProp(string key, string value, ResourceSet r) {
        switch (key) {
            case "mesh": Mesh = r.Get<Mesh>(value); return true;
            case "material": Material = r.Get<Material>(value); return true;
            default: return false;
        }
    }
}

/// <summary>
/// SkinnedMeshRenderer
/// </summary>
public class SkinnedMeshRenderer : MeshRenderer { }

/// <summary>
/// Sprite2D
/// </summary>
public class Sprite2D : Component {
    public Sprite Sprite;
    public override void WriteProps(SceneText.Props w) => w.Prop("sprite", Sprite?.Name);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "sprite") return false; Sprite = r.Get<Sprite>(value); return true; }
}

/// <summary>
/// Sprite3D
/// </summary>
public class Sprite3D : Sprite2D { }

/// <summary>
/// Light
/// </summary>
public class Light : Component {
    public float Radius;
    public Color Color = Color.White;
    public bool Indoors;
    public override void WriteProps(SceneText.Props w) { w.Prop("radius", Radius); w.Prop("color", Color); w.Flag("indoors", Indoors); }
    public override bool ReadProp(string key, string value, ResourceSet r) {
        switch (key) {
            case "radius": Radius = SceneText.ParseFloat(value); return true;
            case "color": Color = SceneText.ParseColor(value); return true;
            case "indoors": Indoors = SceneText.ParseBool(value); return true;
            default: return false;
        }
    }
}

/// <summary>
/// LightProbe
/// </summary>
public class LightProbe : Component { }

/// <summary>
/// Camera
/// </summary>
public class Camera : Component {
    public float Fov = 60f;
    public override void WriteProps(SceneText.Props w) => w.Prop("fov", Fov, 60f);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "fov") return false; Fov = SceneText.ParseFloat(value); return true; }
}

/// <summary>
/// Terrain
/// </summary>
public class Terrain : Component {
    public TerrainData Data;
    public override void WriteProps(SceneText.Props w) => w.Prop("data", Data?.Name);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "data") return false; Data = r.Get<TerrainData>(value); return true; }
}

/// <summary>
/// AudioSource
/// </summary>
public class AudioSource : Component {
    public Audio Audio;
    public bool Playing;
    public override void WriteProps(SceneText.Props w) { w.Prop("audio", Audio?.Name); w.Flag("playing", Playing); }
    public override bool ReadProp(string key, string value, ResourceSet r) {
        switch (key) {
            case "audio": Audio = r.Get<Audio>(value); return true;
            case "playing": Playing = SceneText.ParseBool(value); return true;
            default: return false;
        }
    }
}

#endregion

#region Physics

/// <summary>
/// Collider
/// </summary>
public abstract class Collider : Component { }

/// <summary>
/// MeshCollider
/// </summary>
public class MeshCollider : Collider {
    public Mesh Mesh;
    public override void WriteProps(SceneText.Props w) => w.Prop("mesh", Mesh?.Name);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "mesh") return false; Mesh = r.Get<Mesh>(value); return true; }
}

/// <summary>
/// BoxCollider
/// </summary>
public class BoxCollider : Collider {
    public Vector3 Size = Vector3.One;
    public override void WriteProps(SceneText.Props w) => w.Prop("size", Size, Vector3.One);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "size") return false; Size = SceneText.ParseVector3(value); return true; }
}

/// <summary>
/// Rigidbody
/// </summary>
public class Rigidbody : Component {
    public bool IsKinematic;
    public float Mass = 1f;
    public override void WriteProps(SceneText.Props w) { w.Flag("kinematic", IsKinematic); w.Prop("mass", Mass, 1f); }
    public override bool ReadProp(string key, string value, ResourceSet r) {
        switch (key) {
            case "kinematic": IsKinematic = SceneText.ParseBool(value); return true;
            case "mass": Mass = SceneText.ParseFloat(value); return true;
            default: return false;
        }
    }
}

#endregion

#region Script

/// <summary>
/// Script - a component whose behaviour is supplied inline. Handy in tests:
/// node.AddComponent(new Script { OnProcess = d => ... }).
/// </summary>
public class Script : Component {
    public string Name;
    public System.Action OnReady;
    public System.Action<float> OnProcess;
    public override void Ready() => OnReady?.Invoke();
    public override void Process(float delta) => OnProcess?.Invoke(delta);
    public override void WriteProps(SceneText.Props w) => w.Prop("name", Name);
    public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "name") return false; Name = value; return true; }
}

#endregion
