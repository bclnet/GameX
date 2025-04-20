using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Khronos.Collada;

#region Profile_BRIDGE

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_BRIDGE", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_BRIDGE : Collada_Profile
{
    [XmlAttribute("platform")] public string Platform;
    [XmlAttribute("url")] public string URL;
}

#endregion

#region Profile_CG

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Pass_CG : Collada_Pass
{
    [XmlElement(ElementName = "program")] public Collada_Program_CG Program;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_CG", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_CG : Collada_Profile
{
    [XmlAttribute("platform")] public string Platform;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique")] public Collada_Technique_CG[] Technique;
    [XmlElement(ElementName = "code")] public Collada_Code[] Code;
    [XmlElement(ElementName = "include")] public Collada_Include[] Include;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Program_CG
{
    [XmlElement(ElementName = "shader")] public Collada_Shader_CG[] Shader;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shader_CG : Collada_Shader
{
    [XmlElement(ElementName = "bind_uniform")] public Collada_Bind_Uniform[] Bind_Uniform;
    [XmlElement(ElementName = "compiler")] public Collada_Compiler[] Compiler;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_CG : Collada_Effect_Technique
{
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "pass")] public Collada_Pass_CG[] Pass;
}

#endregion

#region Profile_COMMON

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Effect_Technique_COMMON : Collada_Effect_Technique
{
    [XmlElement(ElementName = "blinn")] public Collada_Blinn Blinn;
    [XmlElement(ElementName = "constant")] public Collada_Constant Constant;
    [XmlElement(ElementName = "lambert")] public Collada_Lambert Lambert;
    [XmlElement(ElementName = "phong")] public Collada_Phong Phong;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_COMMON", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_COMMON : Collada_Profile
{
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique")] public Collada_Effect_Technique_COMMON Technique;
}

#endregion

#region Profile_GLES

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Pass_GLES : Collada_Pass
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLES", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_GLES : Collada_Profile
{
    [XmlAttribute("platform")] public string Platform;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique")] public Collada_Technique_GLES[] Technique;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_GLES : Collada_Effect_Technique
{
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "pass")] public Collada_Pass_GLES[] Pass;
}

#endregion

#region Profile_GLES2

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Pass_GLES2 : Collada_Pass
{
    [XmlElement(ElementName = "program")] public Collada_Program_GLES2 Program;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLES2", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_GLES2 : Collada_Profile
{
    [XmlAttribute("platform")] public string Platform;
    [XmlAttribute("language")] public string Language;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique")] public Collada_Technique_GLES2[] Technique;
    [XmlElement(ElementName = "code")] public Collada_Code[] Code;
    [XmlElement(ElementName = "include")] public Collada_Include[] Include;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Program_GLES2
{
    [XmlElement(ElementName = "linker")] public Collada_Linker[] Linker;
    [XmlElement(ElementName = "shader")] public Collada_Shader_GLES2[] Shader;
    [XmlElement(ElementName = "bind_attribute")] public Collada_Bind_Attribute[] Bind_Attribute;
    [XmlElement(ElementName = "bind_uniform")] public Collada_Bind_Uniform[] Bind_Uniform;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shader_GLES2 : Collada_Shader
{
    [XmlElement(ElementName = "compiler")] public Collada_Compiler[] Compiler;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_GLES2 : Collada_Effect_Technique
{
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "pass")] public Collada_Pass_GLES2[] Pass;
}

#endregion

#region Profile_GLSL

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Pass_GLSL : Collada_Pass
{
    [XmlElement(ElementName = "program")] public Collada_Program_GLSL Program;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLSL", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile_GLSL : Collada_Profile
{
    [XmlAttribute("platform")] public string Platform;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "technique")] public Collada_Technique_GLSL[] Technique;
    [XmlElement(ElementName = "code")] public Collada_Code[] Code;
    [XmlElement(ElementName = "include")] public Collada_Include[] Include;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Program_GLSL
{
    [XmlElement(ElementName = "shader")] public Collada_Shader_GLSL[] Shader;
    [XmlElement(ElementName = "bind_attribute")] public Collada_Bind_Attribute[] Bind_Attribute;
    [XmlElement(ElementName = "bind_uniform")] public Collada_Bind_Uniform[] Bind_Uniform;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shader_GLSL : Collada_Shader
{
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_GLSL : Collada_Effect_Technique
{
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "pass")] public Collada_Pass_GLSL[] Pass;
}

#endregion

#region Profile

[Serializable, XmlType(AnonymousType = true), XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Profile
{
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Custom_Types

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Array_Length
{
    [XmlAttribute("length")] public int Length;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Constant_Attribute
{
    [XmlAttribute("value")] public string Value_As_String;
    [XmlAttribute("param")] public string Param_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Format_Hint
{
    [XmlAttribute("channels")] public Collada_Format_Hint_Channels Channels;
    [XmlAttribute("range")] public Collada_Format_Hint_Range Range;
    [XmlAttribute("precision"), DefaultValue(Collada_Format_Hint_Precision.DEFAULT)] public Collada_Format_Hint_Precision Precision;
    [XmlAttribute("space")] public string Hint_Space;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Hex
{
    [XmlAttribute("format")] public string Format;
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Mips_Attribute
{
    [XmlAttribute("levels")] public int Levels;
    [XmlAttribute("auto_generate")] public bool Auto_Generate;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Ref_String
{
    [XmlAttribute("ref")] public string Ref;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Renderable_Share
{
    [XmlAttribute("share")] public bool Share;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Size_2D
{
    [XmlAttribute("width")] public int Width;
    [XmlAttribute("height")] public int Height;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Size_3D
{
    [XmlAttribute("width")] public int Width;
    [XmlAttribute("height")] public int Height;
    [XmlAttribute("depth")] public int Depth;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size_ratio", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Size_Ratio
{
    [XmlAttribute("width")] public float Width;
    [XmlAttribute("height")] public float Height;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Size_Width_Only
{
    [XmlAttribute("width")] public int Width;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_override", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Override
{
    [XmlAttribute("ref")] public string Ref;
    [XmlAttribute("pass")] public string Pass;
}


[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texcoord", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_TexCoord_Semantic
{
    [XmlAttribute("semantic")] public string Semantic;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texture", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Texture
{
    [XmlAttribute("texture")] public string Texture;
    [XmlAttribute("texcoord")] public string TexCoord;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Effects

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Annotate
{
    [XmlAttribute("name")] public string Name;
    [XmlAnyElement] public XmlElement[] Data;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_vertex_input", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Vertex_Input
{
    [XmlAttribute("semantic")] public string Semantic;
    [XmlAttribute("imput_semantic")] public string Imput_Semantic;
    [XmlAttribute("input_set")] public int Input_Set;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Effect
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "profile_BRIDGE")] public Collada_Profile_BRIDGE[] Profile_BRIDGE;
    [XmlElement(ElementName = "profile_CG")] public Collada_Profile_CG[] Profile_CG;
    [XmlElement(ElementName = "profile_GLES")] public Collada_Profile_GLES[] Profile_GLES;
    [XmlElement(ElementName = "profile_GLES2")] public Collada_Profile_GLES2[] Profile_GLES2;
    [XmlElement(ElementName = "profile_GLSL")] public Collada_Profile_GLSL[] Profile_GLSL;
    [XmlElement(ElementName = "profile_COMMON")] public Collada_Profile_COMMON[] Profile_COMMON;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Effect_Technique
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("id")] public string id;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Effect
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "technique_hint")] public Collada_Technique_Hint[] Technique_Hint;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_effects", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Effects
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "effect")] public Collada_Effect[] Effect;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_hint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Hint
{
    [XmlAttribute("platform")] public string Platform;
    [XmlAttribute("ref")] public string Ref;
    [XmlAttribute("profile")] public string Profile;
}

#endregion

#region Materials

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Materials
{
    [XmlAttribute("semantic")] public string Semantic;
    [XmlAttribute("target")] public string Target;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Material
{
    [XmlElement(ElementName = "param")] public Collada_Param[] Param;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Bind_Material Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Material_Geometry
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("target")] public string Target;
    [XmlAttribute("symbol")] public string Symbol;
    [XmlElement(ElementName = "bind")] public Materials[] Bind;
    [XmlElement(ElementName = "bind_vertex_input")] public Collada_Bind_Vertex_Input[] Bind_Vertex_Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_materials", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Materials
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "material")] public Collada_Material[] Material;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Material
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "instance_effect")] public Collada_Instance_Effect Instance_Effect;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Parameters

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "array", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Array
{
    [XmlAttribute("length")] public int Length;
    [XmlAttribute("resizable")] public bool Resizable;
    [XmlAnyElement] public XmlElement[] Data;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "modifier", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Modifier
{
    [XmlText(), DefaultValue(Collada_Modifier_Value.CONST)] public Collada_Modifier_Value Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sampler_Image : Collada_Instance_Image
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler_states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sampler_States : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "semantic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Semantic
{
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "usertype", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_UserType
{
    [XmlAttribute("typename")] public string TypeName;
    [XmlAttribute("source")] public string Source;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] SetParam;
}

#endregion

#region Rendering

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "blinn", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Blinn
{
    [XmlElement(ElementName = "emission")] public Collada_FX_Common_Color_Or_Texture_Type Eission;
    [XmlElement(ElementName = "ambient")] public Collada_FX_Common_Color_Or_Texture_Type Ambient;
    [XmlElement(ElementName = "diffuse")] public Collada_FX_Common_Color_Or_Texture_Type Diffuse;
    [XmlElement(ElementName = "specular")] public Collada_FX_Common_Color_Or_Texture_Type Specular;
    [XmlElement(ElementName = "transparent")] public Collada_FX_Common_Color_Or_Texture_Type Transparent;
    [XmlElement(ElementName = "reflective")] public Collada_FX_Common_Color_Or_Texture_Type Reflective;
    [XmlElement(ElementName = "shininess")] public Collada_FX_Common_Float_Or_Param_Type Shininess;
    [XmlElement(ElementName = "reflectivity")] public Collada_FX_Common_Float_Or_Param_Type Reflectivity;
    [XmlElement(ElementName = "transparency")] public Collada_FX_Common_Float_Or_Param_Type Transparency;
    [XmlElement(ElementName = "index_of_refraction")] public Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "color_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Color_Clear : Collada_Float_Array_String
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "color_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Color_Target
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
    [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
    [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
    [XmlAttribute("face")][DefaultValue(Collada_Face.POSITIVE_X)] public Collada_Face Face;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "instance_image")] public Collada_Instance_Image Instance_Image;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "constant", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Constant
{
    [XmlElement(ElementName = "emission")] public Collada_FX_Common_Color_Or_Texture_Type Eission;
    [XmlElement(ElementName = "reflective")] public Collada_FX_Common_Color_Or_Texture_Type Reflective;
    [XmlElement(ElementName = "reflectivity")] public Collada_FX_Common_Float_Or_Param_Type Reflectivity;
    [XmlElement(ElementName = "transparent")] public Collada_FX_Common_Color_Or_Texture_Type Transparent;
    [XmlElement(ElementName = "transparency")] public Collada_FX_Common_Float_Or_Param_Type Transparency;
    [XmlElement(ElementName = "index_of_refraction")] public Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "depth_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Depth_Clear
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
    [XmlText()] public float Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "depth_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Depth_Target
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
    [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
    [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
    [XmlAttribute("face"), DefaultValue(Collada_Face.POSITIVE_X)] public Collada_Face Face;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "instance_image")] public Collada_Instance_Image Instance_Image;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "draw", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Draw
{
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "evaluate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Effect_Technique_Evaluate
{
    [XmlElement(ElementName = "color_target")] public Collada_Color_Target Color_Target;
    [XmlElement(ElementName = "depth_target")] public Collada_Depth_Target Depth_Target;
    [XmlElement(ElementName = "stencil_target")] public Collada_Stencil_Target Stencil_Target;
    [XmlElement(ElementName = "color_clear")] public Collada_Color_Clear Color_Clear;
    [XmlElement(ElementName = "depth_clear")] public Collada_Depth_Clear Depth_Clear;
    [XmlElement(ElementName = "stencil_clear")] public Collada_Stencil_Clear Stencil_Clear;
    [XmlElement(ElementName = "draw")] public Collada_Draw Draw;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_common_color_or_texture_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_FX_Common_Color_Or_Texture_Type
{
    [XmlAttribute("opaque"), DefaultValue(Collada_FX_Opaque_Channel.A_ONE)] public Collada_FX_Opaque_Channel Opaque;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "color")] public Collada_Color Color;
    [XmlElement(ElementName = "texture")] public Collada_Texture Texture;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_common_float_or_param_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_FX_Common_Float_Or_Param_Type
{
    [XmlElement(ElementName = "float")] public Collada_SID_Float Float;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Material_Rendering
{
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "technique_override")] public Collada_Technique_Override Technique_Override;
    [XmlElement(ElementName = "bind")] public Materials[] Bind;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "lambert", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Lambert
{
    [XmlElement(ElementName = "emission")] public Collada_FX_Common_Color_Or_Texture_Type Eission;
    [XmlElement(ElementName = "ambient")] public Collada_FX_Common_Color_Or_Texture_Type Ambient;
    [XmlElement(ElementName = "diffuse")] public Collada_FX_Common_Color_Or_Texture_Type Diffuse;
    [XmlElement(ElementName = "reflective")] public Collada_FX_Common_Color_Or_Texture_Type Reflective;
    [XmlElement(ElementName = "transparent")] public Collada_FX_Common_Color_Or_Texture_Type Transparent;
    [XmlElement(ElementName = "reflectivity")] public Collada_FX_Common_Float_Or_Param_Type Reflectivity;
    [XmlElement(ElementName = "transparency")] public Collada_FX_Common_Float_Or_Param_Type Transparency;
    [XmlElement(ElementName = "index_of_refraction")] public Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Pass
{
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "annotate")] public Collada_Annotate[] Annotate;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "states")] public Collada_States States;
    [XmlElement(ElementName = "evaluate")] public Collada_Effect_Technique_Evaluate Evaluate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "phong", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Phong
{
    [XmlElement(ElementName = "emission")] public Collada_FX_Common_Color_Or_Texture_Type Emission;
    [XmlElement(ElementName = "ambient")] public Collada_FX_Common_Color_Or_Texture_Type Ambient;
    [XmlElement(ElementName = "diffuse")] public Collada_FX_Common_Color_Or_Texture_Type Diffuse;
    [XmlElement(ElementName = "specular")] public Collada_FX_Common_Color_Or_Texture_Type Specular;
    [XmlElement(ElementName = "transparent")] public Collada_FX_Common_Color_Or_Texture_Type Transparent;
    [XmlElement(ElementName = "reflective")] public Collada_FX_Common_Color_Or_Texture_Type Reflective;
    [XmlElement(ElementName = "shininess")] public Collada_FX_Common_Float_Or_Param_Type Shininess;
    [XmlElement(ElementName = "reflectivity")] public Collada_FX_Common_Float_Or_Param_Type Reflectivity;
    [XmlElement(ElementName = "transparency")] public Collada_FX_Common_Float_Or_Param_Type Transparency;
    [XmlElement(ElementName = "index_of_refraction")] public Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "render", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Render
{
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("sid")] public string sid;
    [XmlAttribute("camera_node")] public string Camera_Node;
    [XmlElement(ElementName = "layer")] public string[] Layer;
    [XmlElement(ElementName = "instance_material")] public Collada_Instance_Material_Rendering Instance_Material;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_States
{
    [XmlAnyElement] public XmlElement[] Data;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "stencil_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Stencil_Clear
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "stencil_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Stencil_Target
{
    [XmlAttribute("index"), DefaultValue(typeof(int), "1")] public int Index;
    [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
    [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
    [XmlAttribute("face"), DefaultValue(Collada_Face.POSITIVE_X)] public Collada_Face Face;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "instance_image")] public Collada_Instance_Image Instance_Image;
}

#endregion

#region Shaders

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "binary", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Binary
{
    [XmlElement(ElementName = "ref")] public string Ref;
    [XmlElement(ElementName = "hex")] public Collada_Hex Hex;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_attribute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Attribute
{
    [XmlAttribute("symbol")] public string Symbol;
    [XmlElement(ElementName = "semantic")] public Collada_Semantic Semantic;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_uniform", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Uniform
{
    [XmlAttribute("symbol")] public string Symbol;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlAnyElement] public XmlElement[] Data;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "code", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Code
{
    [XmlAttribute("sid")] public string sID;
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "compiler", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Compiler
{
    [XmlAttribute("platform")] public string Platform;
    [XmlAttribute("target")] public string Target;
    [XmlAttribute("options")] public string Options;
    [XmlElement(ElementName = "binary")] public Collada_Binary Binary;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "include", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Include
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("url")] public string URL;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "linker", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Linker
{
    [XmlAttribute("platform")] public string Platform;
    [XmlAttribute("target")] public string Target;
    [XmlAttribute("options")] public string Options;
    [XmlElement(ElementName = "binary")] public Collada_Binary[] Binary;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shader
{
    [XmlAttribute("stage"), DefaultValue(Collada_Shader_Stage.VERTEX)] public Collada_Shader_Stage Stage;
    [XmlElement(ElementName = "sources")] public Collada_Shader_Sources Sources;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sources", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Shader_Sources
{
    [XmlAttribute("entry")] public string Entry;
    [XmlElement(ElementName = "inline")] public string[] Inline;
    [XmlElement(ElementName = "import")] public Collada_Ref_String[] Import;
}

#endregion

#region Technique_Common

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Common_Bind_Material : Collada_Technique_Common
{
    [XmlElement(ElementName = "instance_material")] public Collada_Instance_Material_Geometry[] Instance_Material;
}

#endregion

#region Texturing

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "alpha", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Alpha
{
    [XmlAttribute("operator"), DefaultValue(Collada_Alpha_Operator.ADD)] public Collada_Alpha_Operator Operator;
    [XmlAttribute("scale")] public float Scale;
    [XmlElement(ElementName = "argument")] public Collada_Argument_Alpha[] Argument;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Argument_Alpha
{
    [XmlAttribute("source")] public Collada_Argument_Source Source;
    [XmlAttribute("operand"), DefaultValue(Collada_Argument_Alpha_Operand.SRC_ALPHA)] public Collada_Argument_Alpha_Operand Operand;
    [XmlAttribute("sampler")] public string Sampler;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Argument_RGB
{
    [XmlAttribute("source")] public Collada_Argument_Source Source;
    [XmlAttribute("operand"), DefaultValue(Collada_Argument_RGB_Operand.SRC_COLOR)] public Collada_Argument_RGB_Operand Operand;
    [XmlAttribute("sampler")] public string Sampler;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_2d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Create_2D
{
    [XmlElement(ElementName = "size_exact")] public Collada_Size_2D Size_Exact;
    [XmlElement(ElementName = "size_ratio")] public Collada_Size_Ratio Size_Ratio;
    [XmlElement(ElementName = "mips")] public Collada_Mips_Attribute Mips;
    [XmlElement(ElementName = "unnormalized")] public XmlElement Unnormalized;
    [XmlElement(ElementName = "array")] public Collada_Array_Length Array_Length;
    [XmlElement(ElementName = "format")] public Collada_Format Format;
    [XmlElement(ElementName = "init_from")] public Collada_Init_From[] Init_From;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_3d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Create_3D
{
    [XmlElement(ElementName = "size")] public Collada_Size_3D Size;
    [XmlElement(ElementName = "mips")] public Collada_Mips_Attribute Mips;
    [XmlElement(ElementName = "array")] public Collada_Array_Length Array_Length;
    [XmlElement(ElementName = "format")] public Collada_Format Format;
    [XmlElement(ElementName = "init_from")] public Collada_Init_From[] Init_From;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_cube", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Create_Cube
{
    [XmlElement(ElementName = "size")] public Collada_Size_Width_Only Size;
    [XmlElement(ElementName = "mips")] public Collada_Mips_Attribute Mips;
    [XmlElement(ElementName = "array")] public Collada_Array_Length Array_Length;
    [XmlElement(ElementName = "format")] public Collada_Format Format;
    [XmlElement(ElementName = "init_from")] public Collada_Init_From[] Init_From;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "format", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Format
{
    [XmlElement(ElementName = "hint")] public Collada_Format_Hint Hint;
    [XmlElement(ElementName = "exact")] public string Exact;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_sampler_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_FX_Sampler_Common
{
    [XmlElement(ElementName = "texcoord")] public Collada_TexCoord_Semantic TexCoord_Semantic;
    [XmlElement(ElementName = "wrap_s")] public Collada_FX_Sampler_Common_Wrap_Mode Wrap_S; //: [DefaultValue(Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
    [XmlElement(ElementName = "wrap_t")] public Collada_FX_Sampler_Common_Wrap_Mode Wrap_T; //: [DefaultValue(Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
    [XmlElement(ElementName = "wrap_p")] public Collada_FX_Sampler_Common_Wrap_Mode Wrap_P; //: [DefaultValue(Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
    [XmlElement(ElementName = "minfilter")] public Collada_FX_Sampler_Common_Filter_Type MinFilter; //: [DefaultValue(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    [XmlElement(ElementName = "magfilter")] public Collada_FX_Sampler_Common_Filter_Type MagFilter; //: [DefaultValue(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    [XmlElement(ElementName = "mipfilter")] public Collada_FX_Sampler_Common_Filter_Type MipFilter; //: [DefaultValue(Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
    [XmlElement(ElementName = "border_color")] public Collada_Float_Array_String Border_Color;
    [XmlElement(ElementName = "mip_max_level")] public byte Mip_Max_Level;
    [XmlElement(ElementName = "mip_min_level")] public byte Mip_Min_Level;
    [XmlElement(ElementName = "mip_bias")] public float Mip_Bias;
    [XmlElement(ElementName = "max_anisotropy")] public int Max_Anisotropy;
    [XmlElement(ElementName = "instance_image")] public Collada_Instance_Image Instance_Image;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Image
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "renderable")] public Collada_Renderable_Share Renderable_Share;
    [XmlElement(ElementName = "init_from")] public Collada_Init_From Init_From;
    [XmlElement(ElementName = "create_2d")] public Collada_Create_2D Create_2D;
    [XmlElement(ElementName = "create_3d")] public Collada_Create_3D Create_3D;
    [XmlElement(ElementName = "create_cube")] public Collada_Create_Cube Create_Cube;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "init_from", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Init_From
{
    // Commented out parts are not recognized in Blender (and probably not part of Collada 1.4.1)
    //[XmlAttribute("mips_generate")] public bool Mips_Generate;
    //[XmlAttribute("array_index")] public int Array_Index;
    //[XmlAttribute("mip_index")] public int Mip_Index;
    // Uri added to support 1.4.1 formats
    [XmlText()] public string Uri;
    //[XmlAttribute("depth")] public int Depth;
    [XmlAttribute("face"), DefaultValue(Collada_Face.POSITIVE_X)] public Collada_Face Face;
    [XmlElement(ElementName = "ref")] public string Ref;
    [XmlElement(ElementName = "hex")] public Collada_Hex Hex;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Image
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_images", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Images
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "image")] public Collada_Image[] Image;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_RGB
{
    [XmlAttribute("operator"), DefaultValue(Collada_RGB_Operator.ADD)] public Collada_RGB_Operator Operator;
    [XmlAttribute("scale")] public float Scale;
    [XmlElement(ElementName = "argument")] public Collada_Argument_RGB[] Argument;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler1D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sampler1D : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler2D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sampler2D : Collada_FX_Sampler_Common
{
    [XmlElement(ElementName = "source")] public string Source;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler3D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Sampler3D : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerCUBE", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_SamplerCUBE : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerDEPTH", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_SamplerDEPTH : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerRECT", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_SamplerRECT : Collada_FX_Sampler_Common
{
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texcombiner", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_TexCombiner
{
    [XmlElement(ElementName = "constant")] public Collada_Constant_Attribute Constant;
    [XmlElement(ElementName = "RGB")] public Collada_RGB RGB;
    [XmlElement(ElementName = "alpha")] public Collada_Alpha Alpha;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texenv", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_TexEnv
{
    [XmlAttribute("operator")] public Collada_TexEnv_Operator Operator;
    [XmlAttribute("sampler")] public string Sampler;
    [XmlElement(ElementName = "constant")] public Collada_Constant_Attribute Constant;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texture_pipeline", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Texture_Pipeline
{
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "texcombiner")] public Collada_TexCombiner[] TexCombiner;
    [XmlElement(ElementName = "texenv")] public Collada_TexEnv[] TexEnv;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion