using System;
using System.Xml;
using System.Xml.Serialization;

namespace Khronos.Collada;

#region Articulated_Systems

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "articulated_system", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Articulated_System
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "kinematics")] public Collada_Kinematics Kinematics;
    [XmlElement(ElementName = "motion")] public Collada_Motion Motion;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "axis_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Axis_Info
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("axis")] public string Axis;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind
{
    [XmlAttribute("symbol")] public string Symbol;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "float")] public float Float;
    [XmlElement(ElementName = "int")] public int Int;
    [XmlElement(ElementName = "bool")] public bool Bool;
    [XmlElement(ElementName = "SIDREF")] public string SIDREF;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "connect_param", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Connect_Param
{
    [XmlAttribute("ref")] public string Ref;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "effector_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Effector_Info
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "bind")] public Collada_Bind[] Bind;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
    [XmlElement(ElementName = "speed")] public Collada_Common_Float2_Or_Param_Type Speed;
    [XmlElement(ElementName = "acceleration")] public Collada_Common_Float2_Or_Param_Type Acceleration;
    [XmlElement(ElementName = "deceleration")] public Collada_Common_Float2_Or_Param_Type Deceleration;
    [XmlElement(ElementName = "jerk")] public Collada_Common_Float2_Or_Param_Type Jerk;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_object", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public class Collada_Frame_Object
{
    [XmlAttribute("link")] public string Link;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_origin", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public class Collada_Frame_Origin
{
    [XmlAttribute("link")] public string Link;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_tcp", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Frame_TCP
{
    [XmlAttribute("link")] public string Link;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_tip", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Frame_Tip
{
    [XmlAttribute("link")] public string Link;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_articulated_system", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Articulated_System
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "bind")] public Collada_Bind[] Bind;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Kinematics
{
    [XmlElement(ElementName = "instance_kinematics_model")] public Collada_Instance_Kinematics_Model[] Instance_Kinematics_Model;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Kinematics Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_articulated_systems", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Articulated_Systems
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "articulated_system")] public Collada_Articulated_System[] Articulated_System;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "motion", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Motion
{
    [XmlElement(ElementName = "instance_articulated_system")] public Collada_Instance_Articulated_System Instance_Articulated_System;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Motion Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Custom_Types

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Axis_Info_Kinematics : Collada_Axis_Info
{
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "active")] public Collada_Common_Bool_Or_Param_Type Active;
    [XmlElement(ElementName = "locked")] public Collada_Common_Bool_Or_Param_Type Locked;
    [XmlElement(ElementName = "index")] public Collada_Kinematics_Axis_Info_Index[] Index;
    [XmlElement(ElementName = "limits")] public Collada_Kinematics_Axis_Info_Limits Limits;
    [XmlElement(ElementName = "formula")] public Collada_Formula[] Formula;
    [XmlElement(ElementName = "instance_formula")] public Collada_Instance_Formula[] Instance_Formula;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Axis_Info_Motion : Collada_Axis_Info
{
    [XmlElement(ElementName = "bind")] public Collada_Bind[] Bind;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "setparam")] public Collada_New_Param[] Set_Param;
    [XmlElement(ElementName = "speed")] public Collada_Common_Float_Or_Param_Type Speed;
    [XmlElement(ElementName = "acceleration")] public Collada_Common_Float_Or_Param_Type Acceleration;
    [XmlElement(ElementName = "deceleration")] public Collada_Common_Float_Or_Param_Type Deceleration;
    [XmlElement(ElementName = "jerk")] public Collada_Common_Float_Or_Param_Type Jerk;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "index", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Kinematics_Axis_Info_Index : Collada_Common_Int_Or_Param_Type
{
    [XmlAttribute("semantic")] public string Semantic;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Kinematics_Axis_Info_Limits
{
    [XmlElement(ElementName = "min")] public Collada_Common_Float_Or_Param_Type Min;
    [XmlElement(ElementName = "max")] public Collada_Common_Float_Or_Param_Type Max;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Kinematics_Limits
{
    [XmlElement(ElementName = "min")] public Collada_SID_Name_Float Min;
    [XmlElement(ElementName = "max")] public Collada_SID_Name_Float Max;
}

#endregion

#region Joints

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Joint
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "prismatic")] public Collada_Prismatic[] Prismatic;
    [XmlElement(ElementName = "revolute")] public Collada_Revolute[] Revolute;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_joints", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Joints
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "joint")] public Collada_Joint[] Joint;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "prismatic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Prismatic
{
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "axis")] public Collada_SID_Float_Array_String Axis;
    [XmlElement(ElementName = "limits")] public Collada_Kinematics_Limits Limits;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "revolute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Revolute
{
    [XmlAttribute("sid")] public string sID;
    [XmlElement(ElementName = "axis")] public Collada_SID_Float_Array_String Axis;
    [XmlElement(ElementName = "limits")] public Collada_Kinematics_Limits Limits;
}

#endregion

#region Kinematics_Models

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_end", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Attachment_End
{
    [XmlAttribute("joint")] public string Joint;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_full", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Attachment_Full
{
    [XmlAttribute("joint")] public string Joint;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "link")] public Collada_Link Link;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_start", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Attachment_Start
{
    [XmlAttribute("joint")] public string Joint;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Joint
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Kinematics_Model
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "bind")] public Collada_Bind[] Bind;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Kinematics_Model
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "technique_common")] public Collada_Technique_Common_Kinematics_Model Technique_Common;
    [XmlElement(ElementName = "technique")] public Collada_Technique[] Technique;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_kinematics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Kinematics_Models
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "kinematics_model")] public Collada_Kinematics_Model[] Kinematics_Model;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "link", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Link
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "translate")] public Collada_Translate[] Translate;
    [XmlElement(ElementName = "rotate")] public Collada_Rotate[] Rotate;
    [XmlElement(ElementName = "attachment_full")] public Collada_Attachment_Full Attachment_Full;
    [XmlElement(ElementName = "attachment_end")] public Collada_Attachment_End Attachment_End;
    [XmlElement(ElementName = "attachment_start")] public Collada_Attachment_Start Attachment_Start;
}

#endregion

#region Kinematics_Scenes

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_joint_axis", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Joint_Axis
{
    [XmlAttribute("target")] public string Target;
    [XmlElement(ElementName = "axis")] public Collada_Common_SIDREF_Or_Param_Type Axis;
    [XmlElement(ElementName = "value")] public Collada_Common_Float_Or_Param_Type Value;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Bind_Kinematics_Model
{
    [XmlAttribute("node")] public string Node;
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlElement(ElementName = "SIDREF")] public string SIDREF;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Instance_Kinematics_Scene
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("url")] public string URL;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "setparam")] public Collada_Set_Param[] Set_Param;
    [XmlElement(ElementName = "bind_kinematics_model")] public Collada_Bind_Kinematics_Model[] Bind_Kenematics_Model;
    [XmlElement(ElementName = "bind_joint_axis")] public Collada_Bind_Joint_Axis[] Bind_Joint_Axis;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Kinematics_Scene
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "instance_kinematics_model")] public Collada_Instance_Kinematics_Model[] Instance_Kinematics_Model;
    [XmlElement(ElementName = "instance_articulated_system")] public Collada_Instance_Articulated_System[] Instance_Articulated_System;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Library_Kinematics_Scene
{
    [XmlAttribute("id")] public string ID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "kinematics_scene")] public Collada_Kinematics_Scene[] Kinematics_Scene;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Technique_Common

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Common_Kinematics : Collada_Technique_Common
{
    [XmlElement(ElementName = "axis_info")] public Collada_Axis_Info_Kinematics[] Axis_Info;
    [XmlElement(ElementName = "frame_origin")] public Collada_Frame_Origin Frame_Origin;
    [XmlElement(ElementName = "frame_tip")] public Collada_Frame_Tip Frame_Tip;
    [XmlElement(ElementName = "frame_tcp")] public Collada_Frame_TCP Frame_TCP;
    [XmlElement(ElementName = "frame_object")] public Collada_Frame_Object Frame_Object;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Common_Kinematics_Model : Collada_Technique_Common
{
    [XmlElement(ElementName = "newparam")] public Collada_New_Param[] New_Param;
    [XmlElement(ElementName = "joint")] public Collada_Joint[] Joint;
    [XmlElement(ElementName = "instance_joint")] public Collada_Instance_Joint[] Instance_Joint;
    [XmlElement(ElementName = "link")] public Collada_Link[] Link;
    [XmlElement(ElementName = "formula")] public Collada_Formula[] Formula;
    [XmlElement(ElementName = "instance_formula")] public Collada_Instance_Formula[] Instance_Formula;
}

[Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class Collada_Technique_Common_Motion : Collada_Technique_Common
{
    [XmlElement(ElementName = "axis_info")] public Collada_Axis_Info_Motion[] Axis_Info;
    [XmlElement(ElementName = "effector_info")] public Collada_Effector_Info Effector_Info;
}

#endregion