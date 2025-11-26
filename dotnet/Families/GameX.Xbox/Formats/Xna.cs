using OpenStack.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GameX.Xbox.Formats.Xna;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class OptionalAttribute : Attribute { public bool SharedResource = false; }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreAttribute : Attribute { }

#region Type Manager

public abstract class TypeReader(Type type, bool valueType = false, bool canUseObj = false) {
    public Type Type = type;
    public bool ValueType = valueType;
    public bool CanUseObj = canUseObj;
    public virtual void Init(TypeManager manager) { }
    public abstract object Read(ContentReader r, object o);
}

public abstract class TypeReader<T>(bool valueType = false, bool canUseObj = false) : TypeReader(typeof(T), valueType, canUseObj) {
    public override object Read(ContentReader r, object o) => o == null ? Read(r, default) : Read(r, (T)o);
    public abstract T Read(ContentReader r, T o);
}

// Primitive types
[RType("ByteReader")] class ByteReader() : TypeReader<byte>(valueType: true) { public override byte Read(ContentReader r, byte o) => r.ReadByte(); }
[RType("SByteReader")] class SByteReader() : TypeReader<sbyte>(valueType: true) { public override sbyte Read(ContentReader r, sbyte o) => r.ReadSByte(); }
[RType("Int16Reader")] class Int16Reader() : TypeReader<short>(valueType: true) { public override short Read(ContentReader r, short o) => r.ReadInt16(); }
[RType("UInt16Reader")] class UInt16Reader() : TypeReader<ushort>(valueType: true) { public override ushort Read(ContentReader r, ushort o) => r.ReadUInt16(); }
[RType("Int32Reader")] class Int32Reader() : TypeReader<int>(valueType: true) { public override int Read(ContentReader r, int o) => r.ReadInt32(); }
[RType("UInt32Reader")] class UInt32Reader() : TypeReader<uint>(valueType: true) { public override uint Read(ContentReader r, uint o) => r.ReadUInt32(); }
[RType("Int64Reader")] class Int64Reader() : TypeReader<long>(valueType: true) { public override long Read(ContentReader r, long o) => r.ReadInt64(); }
[RType("UInt64Reader")] class UInt64Reader() : TypeReader<ulong>(valueType: true) { public override ulong Read(ContentReader r, ulong o) => r.ReadUInt64(); }
[RType("SingleReader")] class SingleReader() : TypeReader<float>(valueType: true) { public override float Read(ContentReader r, float o) => r.ReadSingle(); }
[RType("DoubleReader")] class DoubleReader() : TypeReader<double>(valueType: true) { public override double Read(ContentReader r, double o) => r.ReadDouble(); }
[RType("BooleanReader")] class BooleanReader() : TypeReader<bool>(valueType: true) { public override bool Read(ContentReader r, bool o) => r.ReadBoolean(); }
[RType("CharReader")] class CharReader() : TypeReader<char>(valueType: true) { public override char Read(ContentReader r, char o) => r.ReadChar(); }
[RType("StringReader")] class StringReader() : TypeReader<string>() { public override string Read(ContentReader r, string o) => r.ReadLV7UString(); }

// System types
[RType("EnumReader")] class EnumReader<T>() : TypeReader<T>() { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(Enum.GetUnderlyingType(typeof(T))); public override T Read(ContentReader r, T o) => r.ReadRawObject<T>(elem); }
[RType("NullableReader")] class NullableReader<T>() : TypeReader<T?>() where T : struct { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override T? Read(ContentReader r, T? o) => r.ReadBoolean() ? (T?)r.ReadObject<T>(elem, default) : default; }
[RType("ArrayReader")] class ArrayReader<T>() : TypeReader<T[]>() { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override T[] Read(ContentReader r, T[] o) => r.ReadL32FArray(z => r.ReadObject<T>(elem, default), obj: o); }
[RType("ListReader")] class ListReader<T>() : TypeReader<List<T>>(canUseObj: false) { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override List<T> Read(ContentReader r, List<T> o) => r.ReadL32FList(z => r.ReadObject<T>(elem, default), obj: o); }
[RType("DictionaryReader")] class DictionaryReader<TKey, TValue>() : TypeReader<Dictionary<TKey, TValue>>(canUseObj: true) { TypeReader key; TypeReader value; public override void Init(TypeManager manager) { key = manager.GetTypeReader(typeof(TKey)); value = manager.GetTypeReader(typeof(TValue)); } public override Dictionary<TKey, TValue> Read(ContentReader r, Dictionary<TKey, TValue> o) => (Dictionary<TKey, TValue>)r.ReadL32FMany(z => r.ReadObject<TKey>(key, default), z => r.ReadObject<TValue>(value, default), obj: o); }
//[RType("MultiArrayReader")] class MultiArrayReader<T>() : TypeReader<T>() { TypeReader elem; public override void Init(TypeManager manager) => elem = manager.GetTypeReader(typeof(T)); public override T[] Read(ContentReader r, Array o) => default; }
[RType("TimeSpanReader")] class TimeSpanReader() : TypeReader<TimeSpan>(valueType: true) { public override TimeSpan Read(ContentReader r, TimeSpan o) => new(r.ReadInt64()); }
[RType("DateTimeReader")] class DateTimeReader() : TypeReader<DateTime>(valueType: true) { public override DateTime Read(ContentReader r, DateTime o) { var v = r.ReadUInt64(); return new DateTime((long)(v & ~(3UL << 62)), (DateTimeKind)((v >> 62) & 3)); } }
[RType("DecimalReader")] class DecimalReader() : TypeReader<decimal>(valueType: true) { public override decimal Read(ContentReader r, decimal o) => r.ReadDecimal(); }
[RType("ExternalReferenceReader")] class ExternalReferenceReader() : TypeReader<string>() { public override string Read(ContentReader r, string o) => r.ReadExternalReference(); }
[RType("ReflectiveReader")]
class ReflectiveReader<T>() : TypeReader(typeof(T)) {
    delegate void ReadElement(ContentReader r, object p);
    List<ReadElement> Readers;
    ConstructorInfo Constructor;
    TypeReader BaseTypeReader;

    public override void Init(TypeManager manager) {
        CanUseObj = Type.IsClass;
        var baseType = Type.BaseType;
        if (baseType != null && baseType != typeof(object)) BaseTypeReader = manager.GetTypeReader(baseType);
        Constructor = Reflect.GetDefaultConstructor(Type);
        var (properties, fields) = Reflect.GetAllPropertiesFields(Type);
        Readers = new List<ReadElement>(fields.Length + properties.Length);
        foreach (var property in properties) { var reader = GetElementReader(manager, property); if (reader != null) Readers.Add(reader); }
        foreach (var field in fields) { var reader = GetElementReader(manager, field); if (reader != null) Readers.Add(reader); }
    }

    static ReadElement GetElementReader(TypeManager manager, MemberInfo member) {
        var (property, field) = (member as PropertyInfo, member as FieldInfo);
        if (property != null && (!property.CanRead || property.GetIndexParameters().Any())) return null;

        // ignore
        if (Attribute.GetCustomAttribute(member, typeof(IgnoreAttribute)) != null) return null;

        // optional
        var optional = (OptionalAttribute)Attribute.GetCustomAttribute(member, typeof(OptionalAttribute));
        if (optional == null) {
            if (property != null) {
                if (property.GetGetMethod()?.IsPublic != true) return null;
                if (!property.CanWrite) {
                    var reader2 = manager.GetTypeReader(property.PropertyType);
                    if (reader2 == null || !reader2.CanUseObj) return null;
                }
            }
            else if (!field.IsPublic || field.IsInitOnly) return null;
        }

        // setter
        Action<object, object> setter; Type elem;
        if (property != null) { elem = property.PropertyType; setter = property.CanWrite ? (o, v) => property.SetValue(o, v, null) : (o, v) => { }; }
        else { elem = field.FieldType; setter = field.SetValue; }

        // shared resources get special treatment.
        if (optional != null && optional.SharedResource) return (r, p) => r.ReadSharedResource<object>(value => setter(p, value));

        // We need to have a reader at this point.
        var reader = manager.GetTypeReader(elem);
        if (reader == null)
            if (elem == typeof(Array)) reader = new ArrayReader<Array>();
            else throw new Exception($"Content reader could not be found for {elem.FullName} type.");

        // We use the construct delegate to pick the correct existing object to be the target of deserialization.
        Func<object, object> construct = property != null && !property.CanWrite ? parent => property.GetValue(parent, null) : parent => null;
        return (r, p) => setter(p, r.ReadObject(reader, construct(p)));
    }

    public override object Read(ContentReader r, object o) {
        var obj = o != null
            ? (T)o
            : (Constructor == null ? (T)Activator.CreateInstance(typeof(T)) : (T)Constructor.Invoke(null));
        BaseTypeReader?.Read(r, o);
        var b = (object)obj; foreach (var reader in Readers) reader(r, b); obj = (T)b;
        return obj;
    }
}

// Math types
[RType("Vector2Reader")] class Vector2Reader() : TypeReader<Vector2>(valueType: true) { public override Vector2 Read(ContentReader r, Vector2 o) => r.ReadVector2(); }
[RType("Vector3Reader")] class Vector3Reader() : TypeReader<Vector3>(valueType: true) { public override Vector3 Read(ContentReader r, Vector3 o) => r.ReadVector3(); }
[RType("Vector4Reader")] class Vector4Reader() : TypeReader<Vector4>(valueType: true) { public override Vector4 Read(ContentReader r, Vector4 o) => r.ReadVector4(); }
[RType("MatrixReader")] class MatrixReader() : TypeReader<Matrix4x4>(valueType: true) { public override Matrix4x4 Read(ContentReader r, Matrix4x4 o) => r.ReadMatrix4x4(); }
[RType("QuaternionReader")] class QuaternionReader() : TypeReader<Quaternion>(valueType: true) { public override Quaternion Read(ContentReader r, Quaternion o) => r.ReadQuaternion(); }
[RType("ColorReader")] class ColorReader() : TypeReader<ByteColor4>(valueType: true) { public override ByteColor4 Read(ContentReader r, ByteColor4 o) => new(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()); }
[RType("PlaneReader")] class PlaneReader() : TypeReader<Plane>(valueType: true) { public override Plane Read(ContentReader r, Plane o) => new(r.ReadVector3(), r.ReadSingle()); }
[RType("PointReader")] class PointReader() : TypeReader<Point>(valueType: true) { public override Point Read(ContentReader r, Point o) => new(r.ReadInt32(), r.ReadInt32()); }
[RType("RectangleReader")] class RectangleReader() : TypeReader<Rectangle>(valueType: true) { public override Rectangle Read(ContentReader r, Rectangle o) => new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()); }
[RType("BoundingBoxReader")] class BoundingBoxReader() : TypeReader<BoundingBox>(valueType: true) { public override BoundingBox Read(ContentReader r, BoundingBox o) => new(r.ReadVector3(), r.ReadVector3()); }
[RType("BoundingSphereReader")] class BoundingSphereReader() : TypeReader<BoundingSphere>(valueType: true) { public override BoundingSphere Read(ContentReader r, BoundingSphere o) => new(r.ReadVector3(), r.ReadSingle()); }
[RType("BoundingFrustumReader")] class BoundingFrustumReader() : TypeReader<BoundingFrustum>(valueType: true) { public override BoundingFrustum Read(ContentReader r, BoundingFrustum o) => new(r.ReadMatrix4x4()); }
[RType("RayReader")] class RayReader() : TypeReader<Ray>(valueType: true) { public override Ray Read(ContentReader r, Ray o) => new(r.ReadVector3(), r.ReadVector3()); }
[RType("CurveReader")] class CurveReader() : TypeReader<Curve>(valueType: true) { public override Curve Read(ContentReader r, Curve o) => new(r.ReadInt32(), r.ReadInt32(), r.ReadL32FArray(z => new Curve.Key(z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadSingle(), z.ReadInt32()))); }

// Graphics types
[RType("TextureReader")] class TextureReader() : TypeReader<Texture>() { public override Texture Read(ContentReader r, Texture o) => o; }
[RType("Texture2DReader")]
class Texture2DReader() : TypeReader<Texture2D>() {
    public override Texture2D Read(ContentReader r, Texture2D o) => new() {
        Format = (SurfaceFormat)r.ReadInt32(),
        Width = r.ReadUInt32(),
        Height = r.ReadUInt32(),
        Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
    };
}
[RType("Texture3DReader")]
class Texture3DReader() : TypeReader<Texture3D>() {
    public override Texture3D Read(ContentReader r, Texture3D o) => new() {
        Format = (SurfaceFormat)r.ReadInt32(),
        Width = r.ReadUInt32(),
        Height = r.ReadUInt32(),
        Depth = r.ReadUInt32(),
        Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
    };
}
[RType("TextureCubeReader")]
class TextureCubeReader() : TypeReader<TextureCube>() {
    public override TextureCube Read(ContentReader r, TextureCube o) => new() {
        Format = (SurfaceFormat)r.ReadInt32(),
        Size = r.ReadUInt32(),
        Face1Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
        Face2Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
        Face3Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
        Face4Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
        Face5Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
        Face6Mips = r.ReadL32FArray(z => r.ReadL32Bytes()),
    };
}
[RType("IndexBufferReader")]
class IndexBufferReader() : TypeReader<IndexBuffer>() {
    public override IndexBuffer Read(ContentReader r, IndexBuffer o) => new() {
        IndexFormat = r.ReadBoolean() ? 16 : 32,
        IndexData = r.ReadL32Bytes(),
    };
}
[RType("VertexBufferReader")]
class VertexBufferReader() : TypeReader<VertexBuffer>() {
    public override VertexBuffer Read(ContentReader r, VertexBuffer o) => new() {
    };
}
[RType("VertexDeclarationReader")]
class VertexDeclarationReader() : TypeReader<VertexDeclaration>() {
    public override VertexDeclaration Read(ContentReader r, VertexDeclaration o) => new() {
    };
}
[RType("EffectReader")]
class EffectReader() : TypeReader<Effect>() {
    public override Effect Read(ContentReader r, Effect o) => new() {
    };
}
[RType("EffectMaterialReader")]
class EffectMaterialReader() : TypeReader<EffectMaterial>() {
    public override EffectMaterial Read(ContentReader r, EffectMaterial o) => new() {
    };
}
[RType("BasicEffectReader")]
class BasicEffectReader() : TypeReader<BasicEffect>() {
    public override BasicEffect Read(ContentReader r, BasicEffect o) => new() {
    };
}
[RType("AlphaTestEffectReader")]
class AlphaTestEffectReader() : TypeReader<AlphaTestEffect>() {
    public override AlphaTestEffect Read(ContentReader r, AlphaTestEffect o) => new() {
    };
}
[RType("DualTextureEffectReader")]
class DualTextureEffectReader() : TypeReader<DualTextureEffect>() {
    public override DualTextureEffect Read(ContentReader r, DualTextureEffect o) => new() {
    };
}
[RType("EnvironmentMapEffectReader")]
class EnvironmentMapEffectReader() : TypeReader<EnvironmentMapEffect>() {
    public override EnvironmentMapEffect Read(ContentReader r, EnvironmentMapEffect o) => new() {
    };
}
[RType("SkinnedEffectReader")]
class SkinnedEffectReader() : TypeReader<SkinnedEffect>() {
    public override SkinnedEffect Read(ContentReader r, SkinnedEffect o) => new() {
    };
}
[RType("SpriteFontReader")]
class SpriteFontReader() : TypeReader<SpriteFont>() {
    public override SpriteFont Read(ContentReader r, SpriteFont o) => new() {
    };
}
[RType("ModelReader")]
class ModelReader() : TypeReader<Model>() {
    public override Model Read(ContentReader r, Model o) => new() {
    };
}

// Media types
[RType("SoundEffectReader")]
class SoundEffectReader() : TypeReader<SoundEffect>() {
    public override SoundEffect Read(ContentReader r, SoundEffect o) => new() {
        Format = r.ReadL32Bytes(),
        Data = r.ReadL32Bytes(),
        LoopStart = r.ReadInt32(),
        LoopLength = r.ReadInt32(),
        Duration = r.ReadInt32(),
    };
}
[RType("SongReader")]
class SongReader() : TypeReader<Song>() {
    public override Song Read(ContentReader r, Song o) => new() {
        Filename = r.ReadLV7UString(),
        Duration = r.ReadObject<int>(),
    };
}
[RType("VideoReader")]
class VideoReader() : TypeReader<Video>() {
    public override Video Read(ContentReader r, Video o) => new() {
        Filename = r.ReadObject<string>(),
        Duration = r.ReadObject<int>(),
        Width = r.ReadObject<int>(),
        Height = r.ReadObject<int>(),
        FramesPerSecond = r.ReadObject<float>(),
        SoundtrackType = (SoundtrackType)r.ReadObject<int>(),
    };
}

public class TypeManager {
    static readonly object Locker = new();
    static readonly Dictionary<Type, TypeReader> ReadersCache = new(255);
    static readonly string AssemblyName = typeof(TypeManager).Assembly.FullName;
    static Dictionary<Type, TypeReader> Readers;
    static readonly bool IsRunningOnNetCore = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib";
    static Dictionary<string, Func<TypeReader>> TypeFactories = []; // Static map of type names to creation functions. Required as iOS requires all types at compile time

    public TypeReader GetTypeReader(Type type) {
        if (type.IsArray && type.GetArrayRank() > 1) type = typeof(Array);
        return Readers.TryGetValue(type, out var reader) ? reader : null;
    }

    static bool Falseflag = false;

    internal TypeReader[] LoadTypeReaders(ContentReader r) {
#pragma warning disable 0219, 0649
        // Trick to prevent the linker removing the code, but not actually execute the code
        if (Falseflag) {
            // Dummy variables required for it to work on iDevices ** DO NOT DELETE **
            // This forces the classes not to be optimized out when deploying to iDevices
            var hByteReader = new ByteReader();
            var hSByteReader = new SByteReader();
            var hDateTimeReader = new DateTimeReader();
            var hDecimalReader = new DecimalReader();
            var hBoundingSphereReader = new BoundingSphereReader();
            var hBoundingFrustumReader = new BoundingFrustumReader();
            var hRayReader = new RayReader();
            var hCharListReader = new ListReader<char>();
            var hRectangleListReader = new ListReader<Rectangle>();
            var hRectangleArrayReader = new ArrayReader<Rectangle>();
            var hVector3ListReader = new ListReader<Vector3>();
            var hStringListReader = new ListReader<StringReader>();
            var hIntListReader = new ListReader<int>();
            var hSpriteFontReader = new SpriteFontReader();
            var hTexture2DReader = new Texture2DReader();
            var hCharReader = new CharReader();
            var hRectangleReader = new RectangleReader();
            var hStringReader = new StringReader();
            var hVector2Reader = new Vector2Reader();
            var hVector3Reader = new Vector3Reader();
            var hVector4Reader = new Vector4Reader();
            var hCurveReader = new CurveReader();
            var hIndexBufferReader = new IndexBufferReader();
            var hBoundingBoxReader = new BoundingBoxReader();
            var hMatrixReader = new MatrixReader();
            var hBasicEffectReader = new BasicEffectReader();
            var hVertexBufferReader = new VertexBufferReader();
            var hAlphaTestEffectReader = new AlphaTestEffectReader();
            //var hEnumSpriteEffectsReader = new EnumReader<SpriteEffects>();
            var hArrayFloatReader = new ArrayReader<float>();
            var hArrayVector2Reader = new ArrayReader<Vector2>();
            var hListVector2Reader = new ListReader<Vector2>();
            var hArrayMatrixReader = new ArrayReader<Matrix4x4>();
            //var hEnumBlendReader = new EnumReader<Graphics.Blend>();
            var hNullableRectReader = new NullableReader<Rectangle>();
            var hEffectMaterialReader = new EffectMaterialReader();
            var hExternalReferenceReader = new ExternalReferenceReader();
            var hSoundEffectReader = new SoundEffectReader();
            var hSongReader = new SongReader();
            var hModelReader = new ModelReader();
            var hInt32Reader = new Int32Reader();
            var hEffectReader = new EffectReader();
            var hSingleReader = new SingleReader();
            // At the moment the Video class doesn't exist on all platforms... Allow it to compile anyway.
#if ANDROID || (IOS && !TVOS) || MONOMAC || (WINDOWS && !OPENGL)
            var hVideoReader = new VideoReader();
#endif
        }
#pragma warning restore 0219, 0649

        // the first content byte i read tells me the number of content readers in this XNB file
        var readerCount = r.ReadVInt7();
        var readers = new TypeReader[readerCount];
        var needsInit = new BitArray(readerCount);
        Readers = new(readerCount);
        lock (Locker) {
            for (var i = 0; i < readerCount; i++) {
                var readerName = r.ReadString();
                if (TypeFactories.TryGetValue(readerName, out var readerFunc)) {
                    readers[i] = readerFunc();
                    needsInit[i] = true;
                }
                else {
                    var readerType = GetType(readerName);
                    if (!ReadersCache.TryGetValue(readerType, out var reader)) {
                        try {
                            reader = Reflect.GetDefaultConstructor(readerType).Invoke(null) as TypeReader;
                        }
                        catch (TargetInvocationException ex) { throw new InvalidOperationException($"Failed to get default constructor for TypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeFactory() with the following failed type string: {readerName}", ex); }
                        needsInit[i] = true;
                        ReadersCache.Add(readerType, reader);
                    }
                    readers[i] = reader;
                }
                var type = readers[i].Type;
                if (type != null && !Readers.ContainsKey(type)) Readers.Add(type, readers[i]);
                r.ReadInt32();
            }
            // initialize any new readers.
            for (var i = 0; i < readers.Length; i++) if (needsInit.Get(i)) readers[i].Init(this);
        }
        return readers;
    }

    public static Type GetType(string type) {
        // needed to support nested types
        var count = type.Split(["[["], StringSplitOptions.None).Length - 1;
        for (var i = 0; i < count; i++) type = Regex.Replace(type, @"\[(.+?), Version=.+?\]", "[$1]");
        // handle non generic types
        if (type.Contains("PublicKeyToken")) type = Regex.Replace(type, @"(.+?), Version=.+?$", "$1");
        type = type.Replace(", Microsoft.Xna.Framework.Graphics", $", {AssemblyName}");
        type = type.Replace(", Microsoft.Xna.Framework.Video", $", {AssemblyName}");
        type = type.Replace(", Microsoft.Xna.Framework", $", {AssemblyName}");
        type = IsRunningOnNetCore ? type.Replace("mscorlib", "System.Private.CoreLib") : type.Replace("System.Private.CoreLib", "mscorlib");
        // handle gamex map
        type = type.Replace("Microsoft.Xna.Framework.Content", "GameX.Xbox.Formats.Xna");
        type = type.Replace(", StardewValley.GameData", ", GameX.Xbox");
        type = type.Replace("StardewValley.GameData", "GameX.Xbox.Formats.StardewValley.GameData");
        return Type.GetType(type) ?? throw new Exception($"Could not find TypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: {type}");
    }
    public static void AddTypeFactory(string type, Func<TypeReader> factory) { if (!TypeFactories.ContainsKey(type)) TypeFactories.Add(type, factory); }
    public static void ClearTypeFactories() => TypeFactories.Clear();
}

public class ContentReader(Stream stream, string assetName, int version, Action<IDisposable> recordDisposableAction) : BinaryReader(stream) {
    readonly string AssetName = assetName;
    readonly int Version = version;
    readonly Action<IDisposable> RecordDisposableAction = recordDisposableAction;
    TypeManager TypeManager;
    TypeReader[] TypeReaders;
    int SharedResourceCount;
    List<KeyValuePair<int, Action<object>>> SharedResourceFixups;

    void RecordDisposable<T>(T result) {
        if (result is not IDisposable disposable) return;
        RecordDisposableAction?.Invoke(disposable);
    }

    public void ReadTypeReaders() {
        TypeManager = new TypeManager();
        TypeReaders = TypeManager.LoadTypeReaders(this);
        SharedResourceCount = this.ReadVInt7();
        SharedResourceFixups = [];
    }

    public object ReadAsset<T>() { ReadTypeReaders(); var s = ReadObject<T>(); ReadSharedResources(); return s; }
    public object ReadAsset<T>(T obj) { ReadTypeReaders(); var s = ReadObject<T>(obj); ReadSharedResources(); return s; }

    public string ReadExternalReference() => this.ReadLV7UString();

    public T ReadObject<T>() => InnerReadObject(default(T));
    public T ReadObject<T>(T obj) => InnerReadObject(obj);
    public T ReadObject<T>(TypeReader reader) { var s = (T)reader.Read(this, default(T)); RecordDisposable(s); return s; }
    public T ReadObject<T>(TypeReader reader, T obj) {
        if (!reader.ValueType) return ReadObject(obj);
        var s = (T)reader.Read(this, obj); RecordDisposable(s); return s;
    }
    T InnerReadObject<T>(T obj) {
        var idx = this.ReadVInt7();
        if (idx == 0) return obj;
        if (idx > TypeReaders.Length) throw new Exception("Incorrect type reader index found.");
        var s = (T)TypeReaders[idx - 1].Read(this, obj); RecordDisposable(s); return s;
    }

    public T ReadRawObject<T>() => ReadRawObject(default(T));
    public T ReadRawObject<T>(TypeReader reader) => ReadRawObject<T>(reader, default);
    public T ReadRawObject<T>(T obj) {
        var type = typeof(T);
        foreach (var reader in TypeReaders) if (reader.Type == type) return ReadRawObject(reader, obj);
        throw new NotSupportedException();
    }
    public T ReadRawObject<T>(TypeReader reader, T obj) => (T)reader.Read(this, obj);

    public void ReadSharedResources() {
        if (SharedResourceCount <= 0) return;
        var sharedResources = this.ReadFArray(z => InnerReadObject<object>(null), SharedResourceCount);
        // fixup shared resources by calling each registered action
        foreach (var fixup in SharedResourceFixups) fixup.Value(SharedResourceFixups[fixup.Key]);
    }
    public void ReadSharedResource<T>(Action<T> fixup) {
        var idx = this.ReadVInt7();
        if (idx > 0) SharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(idx - 1, v => {
            if (v is T t) fixup(t);
            else throw new Exception($"Error loading shared resource. Expected t {typeof(T).Name}, received t {v.GetType().Name}");
        }));
    }
}

#endregion

#region Gfx Objects

public enum SurfaceFormat {
    Color,
    Bgr565,
    Bgra5551,
    Bgra4444,
    Dxt1,
    Dxt3,
    Dxt5,
    NormalizedByte2,
    NormalizedByte4,
    Rgba1010102,
    Rg32,
    Rgba64,
    Alpha8,
    Single,
    Vector2,
    Vector4,
    HalfSingle,
    HalfVector2,
    HalfVector4,
    HdrBlendable
}

public enum VertexElementFormat {
    Single,
    Vector2,
    Vector3,
    Vector4,
    Color,
    Byte4,
    Short2,
    Short4,
    NormalizedShort2,
    NormalizedShort4,
    HalfVector2,
    HalfVector4
}

public enum VertexElementUsage {
    Position,
    Color,
    TextureCoordinate,
    Normal,
    Binormal,
    Tangent,
    BlendIndices,
    BlendWeight,
    Depth,
    Fog,
    PointSize,
    Sample,
    TessellateFactor
}

public enum CompareFunction { Always, Never, Less, LessEqual, Equal, GreaterEqual, Greater, NotEqual }

public class Texture { }

public class Texture2D : Texture, IHaveMetaInfo, ITexture {
    public SurfaceFormat Format;
    public uint Width;
    public uint Height;
    public byte[][] Mips;
    #region ITexture
    int ITexture.Width => (int)Width;
    int ITexture.Height => (int)Height;
    int ITexture.Depth => 0;
    int ITexture.MipMaps => Mips.Length;
    TextureFlags ITexture.TexFlags => 0;
    public T Create<T>(string platform, Func<object, T> func) {
        var buf = Mips[0];
        if (Mips.Length > 1) throw new NotSupportedException();
        var format = Format switch {
            SurfaceFormat.Color => (TextureFormat.RGBA32, TexturePixel.Unknown),
            SurfaceFormat.Bgr565 => (TextureFormat.RGB565, TexturePixel.Reversed),
            SurfaceFormat.Bgra5551 => (TextureFormat.BGRA1555, TexturePixel.Reversed),
            //SurfaceFormat.Bgra4444 => (TextureFormat.X, TexturePixel.Unknown),
            SurfaceFormat.Dxt1 => (TextureFormat.DXT1, TexturePixel.Unknown),
            SurfaceFormat.Dxt3 => (TextureFormat.DXT3, TexturePixel.Unknown),
            SurfaceFormat.Dxt5 => (TextureFormat.DXT5, TexturePixel.Unknown),
            //SurfaceFormat.NormalizedByte2 => (TextureFormat.X, TexturePixel.Unknown),
            //SurfaceFormat.NormalizedByte4 => (TextureFormat.X, TexturePixel.Unknown),
            //SurfaceFormat.Rgba1010102 => (TextureFormat.X, TexturePixel.Unknown),
            //SurfaceFormat.Rg32 => (TextureFormat.RG32, TexturePixel.Unknown),
            //SurfaceFormat.Rgba64 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.Alpha8 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.Single => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.Vector2 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.Vector4 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.HalfSingle => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.HalfVector2 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.HalfVector4 => (TextureFormat.RGBA32, TexturePixel.Unknown),
            //SurfaceFormat.HdrBlendable => (TextureFormat.RGBA32, TexturePixel.Unknown),
            _ => throw new Exception($"Unknown Format: {Format}"),
        };
        return func(new Texture_Bytes(buf, format, null));
    }
    #endregion
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture2D", items: [
                new($"Format: {Format}"),
                new($"Width: {Width}"),
                new($"Height: {Height}"),
                new($"Mips: {Mips.Length}")
            ])];
}

public class Texture3D : Texture, IHaveMetaInfo {
    public SurfaceFormat Format;
    public uint Width;
    public uint Height;
    public uint Depth;
    public byte[][] Mips;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture3D", items: [
                new($"Format: {Format}"),
                new($"Width: {Width}"),
                new($"Height: {Height}"),
                new($"Depth: {Depth}"),
                new($"Mips: {Mips.Length}")
            ])];
}

public class TextureCube : Texture, IHaveMetaInfo {
    public SurfaceFormat Format;
    public uint Size;
    public byte[][] Face1Mips;
    public byte[][] Face2Mips;
    public byte[][] Face3Mips;
    public byte[][] Face4Mips;
    public byte[][] Face5Mips;
    public byte[][] Face6Mips;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("TextureCube", items: [
                new($"Format: {Format}"),
                new($"Size: {Size}")
            ])];
}

public class IndexBuffer {
    public int IndexFormat;
    public byte[] IndexData;
}

public class VertexDeclaration {
    public class Element {
        public uint Offset = r.ReadUInt32();
        public VertexElementFormat Format = (VertexElementFormat)r.ReadInt32();
        public VertexElementUsage Usage = (VertexElementUsage)r.ReadInt32();
        public uint UsageIndex = r.ReadUInt32();
    }
    public readonly uint VertexStride = r.ReadUInt32();
    public readonly Element[] Elements = r.ReadL32FArray(z => new Element(r));
}

public class VertexBuffer : VertexDeclaration {
    public byte[][] Vertexs;
    public VertexBuffer(ContentReader r) : base(r) {
        Vertexs = r.ReadL32FArray(z => r.ReadBytes(VertexStride));
    }
}

public class Effect {
    public byte[] EffectBytecode = r.ReadL32Bytes();
}

public class EffectMaterial {
    public string EffectReference = r.ReadLV7UString();
    public Dictionary<string, object> Parameters = r.ReadObject<Dictionary<string, object>>();
}

public class BasicEffect {
    public string TextureReference = r.ReadLV7UString();
    public Vector3 DiffuseColor = r.ReadVector3();
    public Vector3 EmissiveColor = r.ReadVector3();
    public Vector3 SpecularColor = r.ReadVector3();
    public float SpecularPower = r.ReadSingle();
    public float Alpha = r.ReadSingle();
    public bool VertexColorEnabled = r.ReadBoolean();
}

public class AlphaTestEffect {
    public string TextureReference = r.ReadLV7UString();
    public CompareFunction AlphaFunction = (CompareFunction)r.ReadInt32();
    public uint ReferenceAlpha = r.ReadUInt32();
    public Vector3 DiffuseColor = r.ReadVector3();
    public float Alpha = r.ReadSingle();
    public bool VertexColorEnabled = r.ReadBoolean();
}

public class DualTextureEffect {
    public string Texture1Reference = r.ReadLV7UString();
    public string Texture2Reference = r.ReadLV7UString();
    public Vector3 DiffuseColor = r.ReadVector3();
    public float Alpha = r.ReadSingle();
    public bool VertexColorEnabled = r.ReadBoolean();
}

public class EnvironmentMapEffect {
    public string TextureReference = r.ReadLV7UString();
    public string EnvironmentMapReference = r.ReadLV7UString();
    public float EnvironmentMapAmount = r.ReadSingle();
    public Vector3 EnvironmentMapSpecular = r.ReadVector3();
    public float FresnelFactor = r.ReadSingle();
    public Vector3 DiffuseColor = r.ReadVector3();
    public Vector3 EmissiveColor = r.ReadVector3();
    public float Alpha = r.ReadSingle();
}

public class SkinnedEffect {
    public string TextureReference = r.ReadLV7UString();
    public uint WeightsPerVertex = r.ReadUInt32();
    public Vector3 DiffuseColor = r.ReadVector3();
    public Vector3 EmissiveColor = r.ReadVector3();
    public Vector3 SpecularColor = r.ReadVector3();
    public float SpecularPower = r.ReadSingle();
    public float Alpha = r.ReadSingle();
}

public class SpriteFont : IHaveMetaInfo {
    public Texture2D Texture = r.ReadObject<Texture2D>();
    public List<Rectangle> Glyphs = r.ReadObject<List<Rectangle>>();
    public List<Rectangle> Cropping = r.ReadObject<List<Rectangle>>();
    public string Characters = new([.. r.ReadObject<List<char>>()]);
    public int VerticalLineSpacing = r.ReadInt32();
    public float HorizontalSpacing = r.ReadSingle();
    public List<Vector3> Kerning = r.ReadObject<List<Vector3>>();
    public char? DefaultCharacter = r.ReadBoolean() ? r.ReadChar() : null;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = Texture }),
            new("SpriteFont", items: [
                new($"Format: {Texture.Format}"),
                new($"Width: {Texture.Width}"),
                new($"Height: {Texture.Height}")
            ])];

    public Vector2 MeasureString(ReadOnlySpan<char> text) {
        if (text == null) throw new ArgumentNullException("text");
        if (text.Length == 0) return Vector2.Zero;
        var res = Vector2.Zero;
        float curLineWidth = 0.0f, finalLineHeight = VerticalLineSpacing;
        var firstInLine = true;
        foreach (char c in text) {
            // special characters
            if (c == '\r') continue;
            if (c == '\n') {
                res.X = Math.Max(res.X, curLineWidth);
                res.Y += VerticalLineSpacing;
                curLineWidth = 0.0f;
                finalLineHeight = VerticalLineSpacing;
                firstInLine = true;
                continue;
            }

            // get the List index from the character map, defaulting to the DefaultCharacter if it's set.
            var index = Characters.IndexOf(c);
            if (index == -1) index = Characters.IndexOf(DefaultCharacter ?? '?');

            // for the first character in a line, always push the width rightward, even if the kerning pushes the character to the left.
            var kern = Kerning[index];
            if (firstInLine) { curLineWidth += Math.Abs(kern.X); firstInLine = false; }
            else curLineWidth += HorizontalSpacing + kern.X;

            // add the character width and right-side bearing to the line width.
            curLineWidth += kern.Y + kern.Z;

            // if a character is taller than the default line height, increase the height to that of the line's tallest character.
            var cropHeight = Cropping[index].Height;
            if (cropHeight > finalLineHeight) finalLineHeight = cropHeight;
        }

        // calculate the final width/height of the text box
        res.X = Math.Max(res.X, curLineWidth);
        res.Y += finalLineHeight;
        return res;
    }
}

public class Model : IHaveMetaInfo {
    static Bone ReadBoneIdx(Model p, ContentReader r) { var id = p.Bones8 ? r.ReadByte() : r.ReadUInt32(); return id != 0 ? p.Bones[(int)(id - 1)] : null; }
    public class Bone(ContentReader r) {
        public string Name = r.ReadObject<string>();
        public Matrix4x4 Transform = r.ReadMatrix4x4();
        public Bone Parent;
        public Bone[] Children;
    }
    public class MeshPart(ContentReader r) {
        public int VertexOffset = r.ReadInt32();
        public int NumVertices = r.ReadInt32();
        public int StartIndex = r.ReadInt32();
        public int PrimitiveCount = r.ReadInt32();
        public object Tag = r.ReadObject<object>();
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public Effect Effect;
        public MeshPart Apply(ContentReader r) {
            r.ReadSharedResource<VertexBuffer>(v => VertexBuffer = v);
            r.ReadSharedResource<IndexBuffer>(v => IndexBuffer = v);
            r.ReadSharedResource<Effect>(v => Effect = v);
            return this;
        }
    }
    public class Mesh(Model p, ContentReader r) {
        public string Name = r.ReadObject<string>();
        public Bone Parent = ReadBoneIdx(p, r);
        public BoundingSphere Bounds = new(r.ReadVector3(), r.ReadSingle());
        public object Tag = r.ReadObject<object>();
        public MeshPart[] Parts = r.ReadL32FArray(z => new MeshPart(r).Apply(r));
    }
    public Bone[] Bones; readonly bool Bones8;
    public Mesh[] Meshs;
    public Bone Root;
    public object Tag;
    public Model(ContentReader r) {
        Bones = r.ReadL32FArray(z => new Bone(r)); Bones8 = Bones.Length < 255;
        foreach (var s in Bones) {
            s.Parent = ReadBoneIdx(this, r);
            s.Children = r.ReadL32FArray(z => ReadBoneIdx(this, r));
        }
        Meshs = r.ReadL32FArray(z => new Mesh(this, r));
        Root = ReadBoneIdx(this, r);
        Tag = r.ReadObject<object>();
    }
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Model", items: [
                new($"Bones: {Bones.Length}"),
                new($"Meshs: {Meshs.Length}"),
                new($"Root: {Root.Name}"),
                new($"Tag: {Tag}")
            ])];
}

#endregion

#region Media Objects

public enum SoundtrackType {
    Music,
    Dialog,
    MusicDialog
}

public class SoundEffect : IHaveMetaInfo {
    public byte[] Format;
    public byte[] Data;
    public int LoopStart;
    public int LoopLength;
    public int Duration;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("SoundEffect", items: [
                new($"Duration: {Duration}")
            ])];
}

public class Song : IHaveMetaInfo {
    public string Filename;
    public int Duration;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Texture", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}")
            ])];
}

public class Video : IHaveMetaInfo {
    public string Filename;
    public int Duration;
    public int Width;
    public int Height;
    public float FramesPerSecond;
    public SoundtrackType SoundtrackType;
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = this }),
            new("Video", items: [
                new($"Filename: {Filename}"),
                new($"Duration: {Duration}"),
                new($"Width: {Width}"),
                new($"Height: {Height}")
            ])];
}

#endregion
