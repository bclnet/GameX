using System;
using System.Xml;
using System.Xml.Serialization;

namespace Khronos.Collada;

#region Analytical_Shape

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "box", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Box
{
    [XmlElement(ElementName = "half_extents")] public Collada_Float_Array_String Half_Extents;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "capsule", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Capsule
{
    [XmlElement(ElementName = "height")] public float Height;
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "convex_mesh", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Convex_Mesh
{
    [XmlAttribute("convex_hull_of")] public string Convex_Hull_Of;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "lines")] public Collada_Lines[] Lines;
    [XmlElement(ElementName = "linestrips")] public Collada_Linestrips[] Linestrips;
    [XmlElement(ElementName = "polygons")] public Collada_Polygons[] Polygons;
    [XmlElement(ElementName = "polylist")] public Collada_Polylist[] Polylist;
    [XmlElement(ElementName = "triangles")] public Collada_Triangles[] Triangles;
    [XmlElement(ElementName = "trifans")] public Collada_Trifans[] Trifans;
    [XmlElement(ElementName = "tristrips")] public Collada_Tristrips[] Tristrips;
    [XmlElement(ElementName = "vertices")] public Collada_Vertices Vertices;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "cylinder", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Cylinder
{
    [XmlElement(ElementName = "height")] public float Height;
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "plane", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Plane
{
    [XmlElement(ElementName = "equation")] public Collada_Float_Array_String Equation;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shape", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shape
{
    [XmlElement(ElementName = "hollow")] public Collada_SID_Bool Hollow;
    [XmlElement(ElementName = "mass")] public Collada_SID_Float Mass;
    [XmlElement(ElementName = "density")] public Collada_SID_Float Density;
    [XmlElement(ElementName = "physics_material")] public Collada_Physics_Material Physics_Material;
    [XmlElement(ElementName = "instance_physics_material")] public Collada_Instance_Physics_Material Instance_Physics_Material;
    [XmlElement(ElementName = "instance_geometry")] public Collada_Instance_Geometry Instance_Geometry;
    [XmlElement(ElementName = "plane")] public Collada_Plane Plane;
    [XmlElement(ElementName = "box")] public Collada_Box Box;
    [XmlElement(ElementName = "sphere")] public Collada_Sphere Sphere;
    [XmlElement(ElementName = "cylinder")] public Collada_Cylinder Cylinder;
    [XmlElement(ElementName = "capsule")] public Collada_Capsule Capsule;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sphere", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sphere
{
    [XmlElement(ElementName = "radius")] public float Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Custom_Types

[Serializable, XmlType(AnonymousType = true), XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Constraint_Limit_Detail
{
    [XmlElement(ElementName = "min")] public Collada_SID_Float_Array_String Min;
    [XmlElement(ElementName = "max")] public Collada_SID_Float_Array_String Max;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Constraint_Limits
{
    [XmlElement(ElementName = "swing_cone_and_twist")] public Collada_Constraint_Limit_Detail Swing_Cone_And_Twist;
    [XmlElement(ElementName = "linear")] public Collada_Constraint_Limit_Detail Linear;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "spring", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Constraint_Spring
{
    [XmlElement(ElementName = "linear")] public Collada_Constraint_Spring_Type Linear;
    [XmlElement(ElementName = "angular")] public Collada_Constraint_Spring_Type Angular;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Constraint_Spring_Type
{
    [XmlElement(ElementName = "stiffness")] public Collada_SID_Float Stiffness;
    [XmlElement(ElementName = "damping")] public Collada_SID_Float Damping;
    [XmlElement(ElementName = "target_value")] public Collada_SID_Float Target_Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "mass_frame", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Mass_Frame
{
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
}

#endregion

#region Physics_Material

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_physics_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Physics_Material
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_physics_materials", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Physics_Materials
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "physics_material")] public Collada_Physics_Material[] Physics_Material;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "physics_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Physics_Material
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Physics_Material Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Physics_Model

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Attachment
{
    [XmlAttribute("rigid_body")] public string Rigid_Body;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Physics_Model
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlAttribute("parent")] public string Parent;
    [XmlElement(ElementName = "instance_force_field")] public Collada_Instance_Force_Field[] Instance_Force_Field;
    [XmlElement(ElementName = "instance_rigid_body")] public Collada_Instance_Rigid_Body[] Instance_Rigid_Body;
    [XmlElement(ElementName = "instance_rigid_constraint")] public Collada_Instance_Rigid_Constraint[] Instance_Rigid_Constraint;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Rigid_Body
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("body")] public string Body;
    [XmlAttribute("target")] public string Target;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Instance_Rigid_Body Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_rigid_constraint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Rigid_Constraint
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("constraint")] public string Constraint;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_physics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Physics_Models
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "physics_model")] public Collada_Physics_Model[] Physics_Model;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Physics_Model
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "rigid_body")] public Collada_Rigid_Body[] Rigid_Body;
    [XmlElement(ElementName = "rigid_constraint")] public Collada_Rigid_Constraint[] Rigid_Constraint;
    [XmlElement(ElementName = "instance_physics_model")] public Collada_Instance_Physics_Model[] Instance_Physics_Model;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "ref_attachment", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Ref_Attachment
{
    [XmlAttribute("rigid_body")] public string Rigid_Body;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Rigid_Body
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Rigid_Body Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "rigid_constraint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Rigid_Constraint
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "ref_attachment")] public Collada_Ref_Attachment Ref_Attachment;
    [XmlElement(ElementName = "attachment")] public Collada_Attachment Attachment;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Rigid_Constraint Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Physics_Scene

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "force_field", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Force_Field
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_force_field", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Force_Field
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_physics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Physics_Scene
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_force_fields", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Force_Fields
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "force_field")] public Collada_Force_Field[] Force_Field;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_physics_scenes", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Physics_Scenes
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "physics_scene")] public Collada_Physics_Scene[] Physics_Scene;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "physics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Physics_Scene
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "instance_force_field")] public Collada_Instance_Force_Field[] Instance_Force_Field;
    [XmlElement(ElementName = "instance_physics_model")] public Collada_Instance_Physics_Model[] Instance_Physics_Model;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Physics_Scene Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Technique_Common

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Common_Instance_Rigid_Body : Collada_Technique_Common
{
    [XmlElement(ElementName = "angular_velocity")] public Collada_Float_Array_String Angular_Velocity;
    [XmlElement(ElementName = "velocity")] public Collada_Float_Array_String Velocity;
    [XmlElement(ElementName = "dynamic")] public Collada_SID_Bool Dynamic;
    [XmlElement(ElementName = "mass")] public Collada_SID_Float Mass;
    [XmlElement(ElementName = "inertia")] public Collada_SID_Float_Array_String Inertia;
    [XmlElement(ElementName = "mass_frame")] public Collada_Mass_Frame Mass_Frame;
    [XmlElement(ElementName = "physics_material")] public Collada_Physics_Material Physics_Material;
    [XmlElement(ElementName = "instance_physics_material")] public Collada_Instance_Physics_Material Instance_Physics_Material;
    [XmlElement(ElementName = "shape")] public Collada_Shape[] Shape;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Physics_Material : Collada_Technique_Common
{
    [XmlElement(ElementName = "dynamic_friction")] public Collada_SID_Float Dynamic_Friction;
    [XmlElement(ElementName = "restitution")] public Collada_SID_Float Restitution;
    [XmlElement(ElementName = "static_friction")] public Collada_SID_Float Static_Friction;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Physics_Scene : Collada_Technique_Common
{
    [XmlElement(ElementName = "gravity")] public Collada_SID_Float_Array_String Gravity;
    [XmlElement(ElementName = "time_step")] public Collada_SID_Float Time_Step;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Rigid_Body : Collada_Technique_Common
{
    [XmlElement(ElementName = "dynamic")] public Collada_SID_Bool Dynamic;
    [XmlElement(ElementName = "mass")] public Collada_SID_Float Mass;
    [XmlElement(ElementName = "inertia")] public Collada_SID_Float_Array_String Inertia;
    [XmlElement(ElementName = "mass_frame")] public Collada_Mass_Frame Mass_Frame;
    [XmlElement(ElementName = "physics_material")] public Collada_Physics_Material Physics_Material;
    [XmlElement(ElementName = "instance_physics_material")] public Collada_Instance_Physics_Material Instance_Physics_Material;
    [XmlElement(ElementName = "shape")] public Collada_Shape[] Shape;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Rigid_Constraint : Collada_Technique_Common
{

    [XmlElement(ElementName = "enabled")] public Collada_SID_Bool Enabled;
    [XmlElement(ElementName = "interpenetrate")] public Collada_SID_Bool Interpenetrate;
    [XmlElement(ElementName = "limits")] public Collada_Constraint_Limits Limits;
    [XmlElement(ElementName = "spring")] public Collada_Constraint_Spring Spring;
}

#endregion