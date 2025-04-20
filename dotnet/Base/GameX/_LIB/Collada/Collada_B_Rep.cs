using System;
using System.Xml;
using System.Xml.Serialization;

namespace Khronos.Collada;

#region Curves

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Circle
{
    [XmlElement(ElementName = "radius")] public float Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Curve
{
    [XmlAttribute("sid")] public string sID;
    [XmlAttribute("name")] public string Name;
    [XmlElement(ElementName = "line")] public Collada_Line Line;
    [XmlElement(ElementName = "circle")] public Collada_Circle Circle;
    [XmlElement(ElementName = "ellipse")] public Collada_Ellipse Ellipse;
    [XmlElement(ElementName = "parabola")] public Collada_Parabola Parabola;
    [XmlElement(ElementName = "hyperbola")] public Collada_Hyperbola Hyperbola;
    [XmlElement(ElementName = "nurbs")] public Collada_Nurbs Nurbs;
    [XmlElement(ElementName = "orient")] public Collada_Orient[] Orient;
    [XmlElement(ElementName = "origin")] public Collada_Origin Origin;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Curves
{
    [XmlElement(ElementName = "curve")] public Collada_Curve[] Curve;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Ellipse
{
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Hyperbola
{
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Line
{
    [XmlElement(ElementName = "origin")] public Collada_Origin Origin;
    [XmlElement(ElementName = "direction")] public Collada_Float_Array_String Direction;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Nurbs
{
    [XmlAttribute("degree")] public int Degree;
    [XmlAttribute("closed")] public bool Closed;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "control_vertices")] public Collada_Control_Vertices Control_Vertices;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Parabola
{
    [XmlElement(ElementName = "focal")] public float Focal;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Surface_Curves
{
    [XmlElement(ElementName = "curve")] public Collada_Curve[] Curve;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Geometry

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_B_Rep
{
    [XmlElement(ElementName = "curves")] public Collada_Curves Curves;
    [XmlElement(ElementName = "surface_curves")] public Collada_Surface_Curves Surface_Curves;
    [XmlElement(ElementName = "surfaces")] public Collada_Surfaces Surfaces;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "vertices")] public Collada_Vertices Vertices;
    [XmlElement(ElementName = "edges")] public Collada_Edges Edges;
    [XmlElement(ElementName = "wires")] public Collada_Wires Wires;
    [XmlElement(ElementName = "faces")] public Collada_Faces Faces;
    [XmlElement(ElementName = "pcurves")] public Collada_PCurves PCurves;
    [XmlElement(ElementName = "shells")] public Collada_Shells Shells;
    [XmlElement(ElementName = "solids")] public Collada_Solids Solids;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Surfaces

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Cone
{
    [XmlElement(ElementName = "radius")] public float Radius;
    [XmlElement(ElementName = "angle")] public float Angle;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Cylinder_B_Rep
{
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Nurbs_Surface
{
    [XmlAttribute("degree_u")] public int Degree_U;
    [XmlAttribute("closed_u")] public bool Closed_U;
    [XmlAttribute("degree_v")] public int Degree_V;
    [XmlAttribute("closed_v")] public bool Closed_V;
    [XmlElement(ElementName = "source")] public Collada_Source[] Source;
    [XmlElement(ElementName = "control_vertices")] public Collada_Control_Vertices Control_Vertices;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Surface
{
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("sid")] public string sID;
    // ggerber 1.4.1 attribue
    [XmlAttribute("type")] public string Type;
    [XmlElement(ElementName = "cone")] public Collada_Cone Cone;
    [XmlElement(ElementName = "plane")] public Collada_Plane Plane;
    [XmlElement(ElementName = "cylinder")] public Collada_Cylinder_B_Rep Cylinder;
    [XmlElement(ElementName = "nurbs_surface")] public Collada_Nurbs_Surface Nurbs_Surface;
    [XmlElement(ElementName = "sphere")] public Collada_Sphere Sphere;
    [XmlElement(ElementName = "torus")] public Collada_Torus Torus;
    [XmlElement(ElementName = "swept_surface")] public Collada_Swept_Surface Swept_Surface;
    [XmlElement(ElementName = "orient")] public Collada_Orient[] Orient;
    [XmlElement(ElementName = "origin")] public Collada_Origin Origin;
    //ggerber 1.4.1
    [XmlElement(ElementName = "init_from")] public Collada_Init_From Init_From;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Surfaces
{
    [XmlElement(ElementName = "surface")] public Collada_Surface[] Surface;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Swept_Surface
{
    [XmlElement(ElementName = "curve")] public Collada_Curve Curve;
    [XmlElement(ElementName = "origin")] public Collada_Origin Origin;
    [XmlElement(ElementName = "direction")] public Collada_Float_Array_String Direction;
    [XmlElement(ElementName = "axis")] public Collada_Float_Array_String Axis;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Torus
{
    [XmlElement(ElementName = "radius")] public Collada_Float_Array_String Radius;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Topology

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Edges
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Faces
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_PCurves
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Shells
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Solids
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Wires
{
    [XmlAttribute("count")] public int Count;
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("id")] public string ID;
    [XmlElement(ElementName = "vcount")] public Collada_Int_Array_String VCount;
    [XmlElement(ElementName = "p")] public Collada_Int_Array_String P;
    [XmlElement(ElementName = "input")] public Collada_Input_Shared[] Input;
    [XmlElement(ElementName = "extra")] public Collada_Extra[] Extra;
}

#endregion

#region Transformation

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Orient : Collada_Float_Array_String
{
}

[Serializable, XmlType(AnonymousType = true)]
public partial class Collada_Origin : Collada_Float_Array_String
{
}

#endregion