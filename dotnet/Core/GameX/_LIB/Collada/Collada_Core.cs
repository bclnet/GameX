using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#pragma warning disable CS0169

namespace Khronos.Collada;

#region Animation

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Animation {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "animation")] public Collada_Animation[] Animation;
    [XmlElement(ElementName = "channel")] public Collada_Channel[] Channel;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "sampler")] public Collada_Sampler[] Sampler;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Animation_Clip {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("start")] public double Start;
    [XmlAttribute("end")] public double End;
    [XmlElement(ElementName = "instance_animation")] public Collada_Instance_Animation[] Instance_Animation;
    [XmlElement(ElementName = "instance_formula")] public Collada_Instance_Formula[] Instance_Formula;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Channel {
    [XmlAttribute("source")] public string Source;
    [XmlAttribute("target")] public string Target;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Animation {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Animation_Clips {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "animation_clip")] public Collada_Animation_Clip[] Animation_Clip;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Animations {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "animation")] public Collada_Animation[] Animation;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Sampler {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("pre_behavior"), DefaultValue(Collada_Sampler_Behavior.UNDEFINED)] public Collada_Sampler_Behavior Pre_Behavior;
    [XmlAttribute("post_behavior"), DefaultValue(Collada_Sampler_Behavior.UNDEFINED)] public Collada_Sampler_Behavior Post_Behavior;
    [XmlElement(ElementName = "input")] public Collada_Input_Unshared[] Input;
}

#endregion

#region Camera

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Camera {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "optics")] public Collada_Optics Optics;
    [XmlElement(ElementName = "imager")] public Collada_Imager Imager;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Imager {
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Camera {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Cameras {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "camera")] public Collada_Camera[] Camera;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Optics {
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Optics Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Orthographic {
    [XmlElement(ElementName = "xmag")] public Collada_SID_Float XMag;
    [XmlElement(ElementName = "ymag")] public Collada_SID_Float YMag;
    [XmlElement(ElementName = "aspect_ratio")] public Collada_SID_Float Aspect_Ratio;
    [XmlElement(ElementName = "znear")] public Collada_SID_Float ZNear;
    [XmlElement(ElementName = "zfar")] public Collada_SID_Float ZFar;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Perspective {
    [XmlElement(ElementName = "xfov")] public Collada_SID_Float XFov;
    [XmlElement(ElementName = "yfov")] public Collada_SID_Float YFov;
    [XmlElement(ElementName = "aspect_ratio")] public Collada_SID_Float Aspect_Ratio;
    [XmlElement(ElementName = "znear")] public Collada_SID_Float ZNear;
    [XmlElement(ElementName = "zfar")] public Collada_SID_Float ZFar;
}

#endregion

#region Controller

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Controller {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "skin")] public Collada_Skin Skin;
    [XmlElement(ElementName = "morph")] public Collada_Morph Morph;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Controller {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "bind_material")] public Collada_Bind_Material[] Bind_Material;
    [XmlElement(ElementName = "skeleton")] public Collada_Skeleton[] Skeleton;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Joints {
    [XmlElement(ElementName = "input")] public Collada_Input_Unshared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Controllers {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "controller")] public Collada_Controller[] Controller;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Morph {
    [XmlAttribute("source")] public string Source_Attribute;
    [XmlAttribute("method")] public string Method;
    [XmlArray("targets")] public Collada_Input_Shared[] Targets;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Skeleton {
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Skin {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("source")] public string source;
    [XmlElement(ElementName = "bind_shape_matrix")] public Collada_Float_Array_String Bind_Shape_Matrix;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "joints")] public Collada_Joints Joints;
    [XmlElement(ElementName = "vertex_weights")] public Collada_Vertex_Weights Vertex_Weights;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Targets {
    [XmlElement(ElementName = "input")] public Collada_Input_Unshared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Vertex_Weights {
    [XmlAttribute("count")] public int Count;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "v")] public Collada_Int_Array_String V;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Custom_Types

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Geographic_Location_Altitude {
    [XmlText()] public float Altitude;
    [XmlAttribute("mode"), DefaultValue(Collada_Geographic_Location_Altitude_Mode.relativeToGround)] public Collada_Geographic_Location_Altitude_Mode Mode;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Poly_PH {
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "h")] public Collada_Int_Array_String[] H;
}

#endregion

#region Data_Flow

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Accessor {
    [XmlAttribute("count")] public uint Count;
    [XmlAttribute("offset")] public uint Offset;
    [XmlAttribute("source")] public string Source;
    [XmlAttribute("stride")] public uint Stride;
    [XmlElement(ElementName = "param")] public Collada_Param[] Param;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Bool_Array : Collada_Bool_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Float_Array : Collada_Float_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("digits"), DefaultValue(typeof(int), "6")] public int Digits;
    [XmlAttribute("magnitude"), DefaultValue(typeof(int), "38")] public int Magnitude;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_IDREF_Array : Collada_String_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Input_Shared : Collada_Input_Unshared {
    [XmlAttribute("offset")] public int Offset;
    [XmlAttribute("set")] public int Set;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Input_Unshared {
    [XmlAttribute("semantic")] public Collada_Input_Semantic Semantic; //: Commenting out default value as it won't write. [DefaultValue(Collada_Input_Semantic.NORMAL)]
    [XmlAttribute("source")] public string source;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Int_Array : Collada_Int_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("minInclusive"), DefaultValue(typeof(int), "-2147483648")] public int Min_Inclusive;
    [XmlAttribute("maxInclusive"), DefaultValue(typeof(int), "2147483647")] public int Max_Inclusive;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Name_Array : Collada_String_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SIDREF_Array : Collada_String_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Source {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "bool_array")] public Collada_Bool_Array Bool_Array;
    [XmlElement(ElementName = "float_array")] public Collada_Float_Array Float_Array;
    [XmlElement(ElementName = "IDREF_array")] public Collada_IDREF_Array IDREF_Array;
    [XmlElement(ElementName = "int_array")] public Collada_Int_Array Int_Array;
    [XmlElement(ElementName = "Name_array")] public Collada_Name_Array Name_Array;
    [XmlElement(ElementName = "SIDREF_array")] public Collada_SIDREF_Array SIDREF_Array;
    [XmlElement(ElementName = "token_array")] public Collada_Token_Array Token_Array;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Source Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    // ggerber 1.4.1 compatibilitiy
    [XmlAttribute("source")] public string Source;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Token_Array : Collada_String_Array_String {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("count")] public int Count;
}

#endregion

#region Extensibility

[Serializable, XmlRoot(ElementName = "bump")]
public partial class Collada_BumpMap {
    [XmlElement(ElementName = "texture")] public Collada_Texture[] Textures { get; set; }
    public static implicit operator XmlElement(Collada_BumpMap bump) {
        var xs = new XmlSerializer(typeof(Collada_BumpMap));
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);
        using var ms = new MemoryStream();
        xs.Serialize(ms, bump, ns);
        ms.Seek(0, SeekOrigin.Begin);
        var doc = new XmlDocument();
        doc.Load(ms);
        return doc.DocumentElement;
    }

    public static implicit operator Collada_BumpMap(XmlElement bump) {
        using var s = new MemoryStream(Encoding.UTF8.GetBytes(bump.OuterXml));
        return (Collada_BumpMap)new XmlSerializer(typeof(Collada_BumpMap)).Deserialize(s);
    }
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Extra {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("type")] public string Type;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique {
    [XmlAttribute("profile")] public string profile;
    [XmlAttribute("xmlns")] public string xmlns;
    [XmlAnyElement] public XmlElement[] Data;
    [XmlElement(ElementName = "bump")] public Collada_BumpMap[] Bump { get; set; }
    [XmlElement(ElementName = "user_properties")] public string UserProperties { get; set; }
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common {
}

#endregion

#region Geometry

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Control_Vertices {
    [XmlElement(ElementName = "input")] public Collada_Input_Unshared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Geometry {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "brep")] public Collada_B_Rep B_Rep;
    [XmlElement(ElementName = "convex_mesh")] public Collada_Convex_Mesh Convex_Mesh;
    [XmlElement(ElementName = "spline")] public Collada_Spline Spline;
    [XmlElement(ElementName = "mesh")] public Collada_Mesh Mesh;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Geometry_Common_Fields {
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("material")] public string Material;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Geometry {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "bind_material")] public Collada_Bind_Material[] Bind_Material;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Geometries {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "geometry")] public Collada_Geometry[] Geometry;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Lines : Collada_Geometry_Common_Fields {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Linestrips : Collada_Geometry_Common_Fields {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Mesh {
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "vertices")] public Collada_Vertices Vertices;
    [XmlElement(ElementName = "lines")] public Collada_Lines[] Lines;
    [XmlElement(ElementName = "linestrips")] public Collada_Linestrips[] Linestrips;
    [XmlElement(ElementName = "polygons")] public Collada_Polygons[] Polygons;
    [XmlElement(ElementName = "polylist")] public Collada_Polylist[] Polylist;
    [XmlElement(ElementName = "triangles")] public Collada_Triangles[] Triangles;
    [XmlElement(ElementName = "trifans")] public Collada_Trifans[] Trifans;
    [XmlElement(ElementName = "tristrips")] public Collada_Tristrips[] Tristrips;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Polygons : Collada_Geometry_Common_Fields {
    [XmlElement(ElementName = "ph")] public Collada_Poly_PH[] PH;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Polylist : Collada_Geometry_Common_Fields {
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Spline {
    [XmlAttribute("closed")] public bool Closed;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "control_vertices")] public Collada_Control_Vertices Control_Vertices;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Triangles : Collada_Geometry_Common_Fields {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Trifans : Collada_Geometry_Common_Fields {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Tristrips : Collada_Geometry_Common_Fields {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Vertices {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "input")] public Collada_Input_Unshared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Lighting

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Ambient {
    [XmlElement(ElementName = "color")] public Collada_Color Color;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Color : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Directional {
    [XmlElement(ElementName = "color")] public Collada_Color Color;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Light {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Lights {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "light")] public Collada_Light[] Light;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Light {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Light Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Point {
    [XmlElement(ElementName = "color")] public Collada_Color Color;
    [XmlElement(ElementName = "constant_attenuation"), DefaultValue(typeof(float), "1.0")] public Collada_SID_Float Constant_Attenuation;
    [XmlElement(ElementName = "linear_attenuation"), DefaultValue(typeof(float), "0.0")] public Collada_SID_Float Linear_Attenuation;
    [XmlElement(ElementName = "quadratic_attenuation")] public Collada_SID_Float Quadratic_Attenuation;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Spot {
    [XmlElement(ElementName = "color")] public Collada_Color Color;
    [XmlElement(ElementName = "constant_attenuation"), DefaultValue(typeof(float), "1.0")] public Collada_SID_Float Constant_Attenuation;
    [XmlElement(ElementName = "linear_attenuation"), DefaultValue(typeof(float), "0.0")] public Collada_SID_Float Linear_Attenuation;
    [XmlElement(ElementName = "quadratic_attenuation"), DefaultValue(typeof(float), "0.0")] public Collada_SID_Float Quadratic_Attenuation;
    [XmlElement(ElementName = "falloff_angle"), DefaultValue(typeof(float), "180.0")] public Collada_SID_Float Falloff_Angle;
    [XmlElement(ElementName = "falloff_exponent"), DefaultValue(typeof(float), "0.0")] public Collada_SID_Float Falloff_Exponent;
}

#endregion

#region Mathematics

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Formula {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Formula Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "target")] public Collada_Common_Float_Or_Param_Type Target;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Formula {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Formulas {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "formula")] public Collada_Formula[] Formula;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Metadata

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Asset {
    [XmlElement(ElementName = "created")] public DateTime Created;
    [XmlElement(ElementName = "modified")] public DateTime Modified;
    [XmlElement(ElementName = "unit")] public Collada_Asset_Unit Unit;
    [XmlElement(ElementName = "up_axis"), DefaultValue("Y_UP")] public string Up_Axis;
    [XmlElement(ElementName = "contributor")] public Collada_Asset_Contributor[] Contributor;
    [XmlElement(ElementName = "keywords")] public string Keywords;
    [XmlElement(ElementName = "revision")] public string Revision;
    [XmlElement(ElementName = "subject")] public string Subject;
    [XmlElement(ElementName = "title")] public string Title;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "coverage")] public Collada_Asset_Coverage Coverage;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Asset_Contributor {
    [XmlElement(ElementName = "author")] public string Author;
    [XmlElement(ElementName = "author_email")] public string Author_Email;
    [XmlElement(ElementName = "author_website")] public string Author_Website;
    [XmlElement(ElementName = "authoring_tool")] public string Authoring_Tool;
    [XmlElement(ElementName = "comments")] public string Comments;
    [XmlElement(ElementName = "copyright")] public string Copyright;
    [XmlElement(ElementName = "source_data")] public string Source_Data;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Asset_Coverage {
    [XmlElement(ElementName = "geographic_location")] Collada_Geographic_Location Geographic_Location;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Asset_Unit {
    [XmlAttribute("meter")] public double Meter; //: [DefaultValue(1.0)] // Commented out to force it to write these values.
    [XmlAttribute("name")] public string Name; //: [DefaultValue("meter")]
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Geographic_Location {
    [XmlElement(ElementName = "longitude")] public float Longitude;
    [XmlElement(ElementName = "latitude")] public float Latitude;
    [XmlElement(ElementName = "altitude")] public Collada_Geographic_Location_Altitude Altitude;
}

#endregion

#region Parameters

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_New_Param {
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "semantic")] public string Semantic;
    [XmlElement(ElementName = "modifier")] public string Modifier;
    [XmlElement("annotate")] public Collada_Annotate[] Annotate;
    [XmlAnyElement] public XmlElement[] Data;
    // ggerber 1.4.1 elements.  Surface and Sampler2D are single elements for textures.
    [XmlElement("surface")] public Collada_Surface Surface;
    [XmlElement("sampler2D")] public Collada_Sampler2D Sampler2D;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Param {
    [XmlAttribute("ref")] public string Ref;
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("semantic")] public string Semantic;
    [XmlAttribute("type")] public string Type;
    [XmlAnyElement] public XmlElement[] Data;
    //TODO: this is used in a few contexts
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Set_Param {
    [XmlAttribute("ref")] public string Ref;
    [XmlAnyElement] public XmlElement[] Data;
}

#endregion

#region Scene

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Evaluate_Scene {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("sid")] public string sid;
    [XmlAttribute("enable")] public bool Enable;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "render")] public Collada_Render[] Render;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Node {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlAttribute("stream")] public string Stream;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Instance_Visual_Scene {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Nodes {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "node")] public Collada_Node[] Node;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Library_Visual_Scenes {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "visual_scene")] public Collada_Visual_Scene[] Visual_Scene;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Node {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("type")] public Collada_Node_Type Type; //: [DefaultValue(Collada_Node_Type.NODE)]
    [XmlAttribute("layer")] public string Layer;
    [XmlElement(ElementName = "lookat")] public Collada_Lookat[] Lookat;
    [XmlElement(ElementName = "matrix")] public Collada_Matrix[] Matrix;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "scale")] public Collada_Scale[] Scale;
    [XmlElement(ElementName = "skew")] public Collada_Skew[] Skew;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "instance_camera")] public Collada_Instance_Camera[] Instance_Camera;
    [XmlElement(ElementName = "instance_controller")] public Collada_Instance_Controller[] Instance_Controller;
    [XmlElement(ElementName = "instance_geometry")] public Collada_Instance_Geometry[] Instance_Geometry;
    [XmlElement(ElementName = "instance_light")] public Collada_Instance_Light[] Instance_Light;
    [XmlElement(ElementName = "instance_node")] public Collada_Instance_Node[] Instance_Node;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "node")] public Collada_Node[] node;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Scene {
    [XmlElement(ElementName = "instance_visual_scene")] public Collada_Instance_Visual_Scene Visual_Scene;
    [XmlElement(ElementName = "instance_physics_scene")] public Collada_Instance_Physics_Scene[] Physics_Scene;
    [XmlElement(ElementName = "instance_kinematics_scene")] public Collada_Instance_Kinematics_Scene Kinematics_Scene;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Visual_Scene {
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "evaluate_scene")] public Collada_Evaluate_Scene[] Evaluate_Scene;
    [XmlElement(ElementName = "node")] public Collada_Node[] Node;
}

#endregion

#region Technique_Common

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Formula : Collada_Technique_Common {
    [XmlAnyElement] public XmlElement[] Data;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Light : Collada_Technique_Common {
    [XmlElement(ElementName = "ambient")] public Collada_Ambient Ambient;
    [XmlElement(ElementName = "directional")] public Collada_Directional Directional;
    [XmlElement(ElementName = "point")] public Collada_Point Point;
    [XmlElement(ElementName = "spot")] public Collada_Spot Spot;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Optics : Collada_Technique_Common {
    [XmlElement(ElementName = "orthographic")] public Collada_Orthographic Orthographic;
    [XmlElement(ElementName = "perspective")] public Collada_Perspective Perspective;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Technique_Common_Source : Collada_Technique_Common {
    [XmlElement(ElementName = "accessor")] public Collada_Accessor Accessor;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
}

#endregion

#region Transform

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Lookat : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Matrix : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Rotate : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Scale : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Skew : Collada_SID_Float_Array_String {
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Translate : Collada_SID_Float_Array_String {
}

#endregion