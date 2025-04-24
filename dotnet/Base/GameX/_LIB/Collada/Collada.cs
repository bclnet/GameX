using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using static OpenStack.Debug;

namespace Khronos.Collada;

#region Collada

//[XmlRoot(ElementName = "COLLADA", Namespace = "https://www.khronos.org/files/collada_schema_1_5", IsNullable = false)]
//[XmlRoot(ElementName = "COLLADA", Namespace = "http://www.khronos.org/files/collada_schema_1_4", IsNullable = false)]
[Serializable, DebuggerStepThrough(), DesignerCategory("code"), XmlType(AnonymousType = true), XmlRoot(ElementName = "COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
public partial class Collada {
    [XmlAttribute("version")] public string Collada_Version;
    [XmlElement(ElementName = "asset")] public Collada_Asset Asset;

    // FX Elements
    [XmlElement(ElementName = "library_images")] public Collada_Library_Images Library_Images;
    [XmlElement(ElementName = "library_effects")] public Collada_Library_Effects Library_Effects;
    [XmlElement(ElementName = "library_materials")] public Collada_Library_Materials Library_Materials;

    // Core Elements
    [XmlElement(ElementName = "library_animations")] public Collada_Library_Animations Library_Animations;
    [XmlElement(ElementName = "library_animation_clips")] public Collada_Library_Animation_Clips Library_Animation_Clips;
    [XmlElement(ElementName = "library_cameras")] public Collada_Library_Cameras Library_Cameras;
    [XmlElement(ElementName = "library_controllers")] public Collada_Library_Controllers Library_Controllers;
    [XmlElement(ElementName = "library_formulas")] public Collada_Library_Formulas Library_Formulas;
    [XmlElement(ElementName = "library_geometries")] public Collada_Library_Geometries Library_Geometries;
    [XmlElement(ElementName = "library_lights")] public Collada_Library_Lights Library_Lights;
    [XmlElement(ElementName = "library_nodes")] public Collada_Library_Nodes Library_Nodes;
    [XmlElement(ElementName = "library_visual_scenes")] public Collada_Library_Visual_Scenes Library_Visual_Scene;

    // Physics Elements
    [XmlElement(ElementName = "library_force_fields")] public Collada_Library_Force_Fields Library_Force_Fields;
    [XmlElement(ElementName = "library_physics_materials")] public Collada_Library_Physics_Materials Library_Physics_Materials;
    [XmlElement(ElementName = "library_physics_models")] public Collada_Library_Physics_Models Library_Physics_Models;
    [XmlElement(ElementName = "library_physics_scenes")] public Collada_Library_Physics_Scenes Library_Physics_Scenes;

    // Kinematics
    [XmlElement(ElementName = "library_articulated_systems")] public Collada_Library_Articulated_Systems Library_Articulated_Systems;
    [XmlElement(ElementName = "library_joints")] public Collada_Library_Joints Library_Joints;
    [XmlElement(ElementName = "library_kinematics_models")] public Collada_Library_Kinematics_Models Library_Kinematics_Models;
    [XmlElement(ElementName = "library_kinematics_scenes")] public Collada_Library_Kinematics_Scene Library_Kinematics_Scene;
    [XmlElement(ElementName = "scene")] public Collada_Scene Scene;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;

    public Collada() {
        Collada_Version = "1.5";
        Asset = new Collada_Asset {
            Title = "Test Engine 1"
        };
        Library_Visual_Scene = new Collada_Library_Visual_Scenes();
    }

    public static Collada Load_File(string file_name) {
        try {
            using (var tr = new StreamReader(file_name))
                return (Collada)new XmlSerializer(typeof(Collada)).Deserialize(tr);
        }
        catch (Exception ex) {
            Log(ex.ToString());
            Console.ReadLine();
            return null;
        }
    }

    //public async static void Save_File(Collada data)
    //{
    //    var savePicker = new FileSavePicker
    //    {
    //        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
    //        SuggestedFileName = "Test",
    //    };
    //    savePicker.FileTypeChoices.Add("DAE", new List<string>() { ".dae" });
    //    savePicker.FileTypeChoices.Add("Xml", new List<string>() { ".xml" });
    //    var file = await savePicker.PickSaveFileAsync();
    //    if (file != null)
    //    {
    //        var sessionRandomAccess = await file.OpenAsync(FileAccessMode.ReadWrite);
    //        var sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
    //        try
    //        {
    //            var serializer = new XmlSerializer(typeof(Collada));
    //            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), data);
    //        }
    //        catch (InvalidOperationException e) { Debug.WriteLine(e.InnerException); }
    //        sessionRandomAccess.Dispose();
    //        await sessionOutputStream.FlushAsync();
    //        sessionOutputStream.Dispose();
    //    }
    //}
}

#endregion

#region Collada_Helpers

public partial class Collada_Bool_Array_String {
    public bool[] Value() => Collada_Parse_Utils.String_To_Bool(Value_As_String);
}

public partial class Collada_Common_Float2_Or_Param_Type {
    public float[] Value() => Collada_Parse_Utils.String_To_Float(Value_As_String);
}

public partial class Collada_Float_Array_String {
    public float[] Value() => Collada_Parse_Utils.String_To_Float(Value_As_String);
}

public partial class Collada_Int_Array_String {
    public int[] Value() => Collada_Parse_Utils.String_To_Int(this.Value_As_String);
}

public class Collada_Parse_Utils {
    public static int[] String_To_Int(string int_array) {
        var str = int_array.Split(' ');
        var array = new int[str.LongLength];
        try {
            for (var i = 0L; i < str.LongLength; i++)
                array[i] = Convert.ToInt32(str[i]);
        }
        catch (Exception e) {
            Log(e.ToString());
            Log(int_array);
        }
        return array;
    }

    public static float[] String_To_Float(string float_array) {
        var str = float_array.Split(' ');
        var array = new float[str.LongLength];
        try {
            for (var i = 0L; i < str.LongLength; i++)
                array[i] = Convert.ToSingle(str[i]);
        }
        catch (Exception e) {
            Log(e.ToString());
            Log(float_array);
        }
        return array;
    }

    public static bool[] String_To_Bool(string bool_array) {
        var str = bool_array.Split(' ');
        var array = new bool[str.LongLength];
        try {
            for (var i = 0L; i < str.LongLength; i++)
                array[i] = Convert.ToBoolean(str[i]);
        }
        catch (Exception e) {
            Log(e.ToString());
            Log(bool_array);
        }
        return array;
    }
}

public partial class Collada_SID_Float_Array_String {
    public float[] Value() => Collada_Parse_Utils.String_To_Float(Value_As_String);
}

public partial class Collada_SID_Int_Array_String {
    public int[] Value() => Collada_Parse_Utils.String_To_Int(Value_As_String);
}

public partial class Collada_String_Array_String {
    public string[] Value() => Value_Pre_Parse.Split(' ');
}

#endregion

#region Enums

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Alpha_Operator {
    REPLACE,
    MODULATE,
    ADD,
    ADD_SIGNED,
    INTERPOLATE,
    SUBTRACT
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Argument_Alpha_Operand {
    SRC_ALPHA,
    ONE_MINUS_SRC_ALPHA
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Argument_RGB_Operand {
    SRC_COLOR,
    ONE_MINUS_SRC_COLOR,
    SRC_ALPHA,
    ONE_MINUS_SRC_ALPHA
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Argument_Source {
    TEXTURE,
    CONSTANT,
    PRIMARY,
    PREVIOUS
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Face {
    POSITIVE_X,
    NEGATIVE_X,
    POSITIVE_Y,
    NEGATIVE_Y,
    POSITIVE_Z,
    NEGATIVE_Z
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Format_Hint_Channels {
    RGB,
    RGBA,
    RGBE,
    L,
    LA,
    D
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Format_Hint_Precision {
    DEFAULT,
    LOW,
    MID,
    HIGH,
    MAX
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Format_Hint_Range {
    SNORM,
    UNORM,
    SINT,
    UINT,
    FLOAT
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_FX_Opaque_Channel {
    A_ONE,
    RGB_ZERO,
    A_ZERO,
    RGB_ONE
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_FX_Sampler_Common_Filter_Type {
    NONE,
    NEAREST,
    LINEAR,
    ANISOTROPIC
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_FX_Sampler_Common_Wrap_Mode {
    WRAP,
    MIRROR,
    CLAMP,
    BORDER,
    MIRROR_ONCE,
    REPEAT,
    CLAMP_TO_EDGE,
    MIRRORED_REPEAT
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Geographic_Location_Altitude_Mode {
    absolute,
    relativeToGround
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Input_Semantic {
    BINORMAL,
    COLOR,
    CONTINUITY,
    IMAGE,
    INPUT,
    IN_TANGENT,
    INTERPOLATION,
    INV_BIND_MATRIX,
    JOINT,
    LINEAR_STEPS,
    MORPH_TARGET,
    MORPH_WEIGHT,
    NORMAL,
    OUTPUT,
    OUT_TANGENT,
    POSITION,
    TANGENT,
    TEXBINORMAL,
    TEXCOORD,
    TEXTANGENT,
    UV,
    VERTEX,
    WEIGHT,
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Modifier_Value {
    CONST,
    UNIFORM,
    VARYING,
    STATIC,
    VOLATILE,
    EXTERN,
    SHARED
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Node_Type {
    JOINT,
    NODE
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_RGB_Operator {
    REPLACE,
    MODULATE,
    ADD,
    ADD_SIGNED,
    INTERPOLATE,
    SUBTRACT,
    DOT3_RGB,
    DOT3_RGBA
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Sampler_Behavior {
    UNDEFINED,
    CONSTANT,
    GRADIENT,
    CYCLE,
    OSCILLATE,
    CYCLE_RELATIVE
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_Shader_Stage {
    TESSELATION,
    VERTEX,
    GEOMETRY,
    FRAGMENT
}

[Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
public enum Collada_TexEnv_Operator {
    REPLACE,
    MODULATE,
    DECAL,
    BLEND,
    ADD
}

#endregion

#region Types

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Bool_Array_String {
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_Bool_Or_Param_Type : Collada_Common_Param_Type {
    [XmlElement(ElementName = "bool")] public bool Bool;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_Float_Or_Param_Type : Collada_Common_Param_Type {
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_Float2_Or_Param_Type {
    [XmlElement(ElementName = "param")] public Collada_Param Param;
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_Int_Or_Param_Type : Collada_Common_Param_Type {
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_Param_Type {
    [XmlElement(ElementName = "param")] public Collada_Param Param;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Common_SIDREF_Or_Param_Type : Collada_Common_Param_Type {
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Float_Array_String {
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Int_Array_String {
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Bool {
    [XmlAttribute("sid")] public string sID;
    [XmlText()] public bool Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Float {
    [XmlAttribute("sid")] public string sID;
    [XmlText()] public float Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Float_Array_String {
    [XmlAttribute("sid")] public string sID;
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Int_Array_String {
    [XmlAttribute("sid")] public string sID;
    [XmlText()] public string Value_As_String;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Name_Float {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlText()] public float Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_SID_Name_String {
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlText()] public string Value;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_String_Array_String {
    [XmlText()] public string Value_Pre_Parse;
}

#endregion
