using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GameX;

/// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to
#region DesSer

public static class DesSer {
    public static Action<JsonTypeInfo> AlphabetizeProperties() {
        return static typeInfo => {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
            var properties = typeInfo.Properties.OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
            typeInfo.Properties.Clear();
            for (var i = 0; i < properties.Count; i++) {
                properties[i].Order = i;
                typeInfo.Properties.Add(properties[i]);
            }
        };
    }
    readonly static JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        IndentSize = 2,
        //DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new TypeInfoResolver {
            Modifiers = { AlphabetizeProperties() }
        }
    };
    static DesSer() {
        Add(new Color3JsonConverter(),
            new ByteColor3JsonConverter(),
            new Color4JsonConverter(),
            new ByteColor4JsonConverter(),
            new FloatJsonConverter(),
            new Vector2JsonConverter(),
            new Vector3JsonConverter(),
            new Vector4JsonConverter(),
            new Matrix2x2JsonConverter(),
            new Matrix3x3JsonConverter(),
            new Matrix3x4JsonConverter(),
            new Matrix4x4JsonConverter());
    }

    public static void Add(params JsonConverter[] converters) { foreach (var s in converters) Options.Converters.Add(s); }

    public static string Serialize<T>(this T source) => JsonSerializer.Serialize(source, Options);
    public static void Serialize<T>(this T source, Stream stream) {
        stream.WriteBytes(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source, Options)));
        //var w = new Utf8JsonWriter(stream);
        //JsonSerializer.Serialize(w, source, Options);
        //w.Flush();
    }

    public static T Deserialize<T>(string source) => JsonSerializer.Deserialize<T>(source, Options);
    public static T Deserialize<T>(Stream stream) {
        var buf = new byte[stream.Length];
        stream.Read(buf, 0, buf.Length);
        return JsonSerializer.Deserialize<T>(buf, Options);
    }
}

public class TypeInfoResolver : DefaultJsonTypeInfoResolver {
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
        var typeInfo = base.GetTypeInfo(type, options);
        if (typeInfo.Kind == JsonTypeInfoKind.Object)
            foreach (var i in typeInfo.Properties)
                if (i.Name == "baseStream") i.ShouldSerialize = (instance, value) => false;
        return typeInfo;
    }
}

public class Color3JsonConverter : JsonConverter<Color3> {
    public override Color3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Color3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R:f4} {s.G:f4} {s.B:f4}");
}

public class ByteColor3JsonConverter : JsonConverter<ByteColor3> {
    public override ByteColor3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, ByteColor3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B}");
}

public class Color4JsonConverter : JsonConverter<Color4> {
    public override Color4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Color4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R:f4} {s.G:f4} {s.B:f4} {s.A:f4}");
}

public class ByteColor4JsonConverter : JsonConverter<ByteColor4> {
    public override ByteColor4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, ByteColor4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B} {s.A}");
}

class FloatJsonConverter : JsonConverter<float> {
    public override float Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, float s, JsonSerializerOptions options) => w.WriteRawValue($"{s:f4}");
}

class Vector2JsonConverter : JsonConverter<Vector2> {
    public override Vector2 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector2 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f4} {s.Y:f4}");
}

class Vector3JsonConverter : JsonConverter<Vector3> {
    public override Vector3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f4} {s.Y:f4} {s.Z:f4}");
}

class Vector4JsonConverter : JsonConverter<Vector4> {
    public override Vector4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f4} {s.Y:f4} {s.Z:f4} {s.W:f4}");
}

class Matrix2x2JsonConverter : JsonConverter<Matrix2x2> {
    public override Matrix2x2 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix2x2 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f4} {s.M12:f4}");
        w.WriteStringValue($"{s.M21:f4} {s.M22:f4}");
        w.WriteEndArray();
    }
}

class Matrix3x3JsonConverter : JsonConverter<Matrix3x3> {
    public override Matrix3x3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix3x3 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f4} {s.M12:f4} {s.M13:f4}");
        w.WriteStringValue($"{s.M21:f4} {s.M22:f4} {s.M23:f4}");
        w.WriteStringValue($"{s.M31:f4} {s.M32:f4} {s.M33:f4}");
        w.WriteEndArray();
    }
}

class Matrix3x4JsonConverter : JsonConverter<Matrix3x4> {
    public override Matrix3x4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix3x4 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f4} {s.M12:f4} {s.M13:f4} {s.M14:f4}");
        w.WriteStringValue($"{s.M21:f4} {s.M22:f4} {s.M23:f4} {s.M24:f4}");
        w.WriteStringValue($"{s.M31:f4} {s.M32:f4} {s.M33:f4} {s.M34:f4}");
        w.WriteEndArray();
    }
}

class Matrix4x4JsonConverter : JsonConverter<Matrix4x4> {
    public override Matrix4x4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix4x4 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f4} {s.M12:f4} {s.M13:f4} {s.M14:f4}");
        w.WriteStringValue($"{s.M21:f4} {s.M22:f4} {s.M23:f4} {s.M24:f4}");
        w.WriteStringValue($"{s.M31:f4} {s.M32:f4} {s.M33:f4} {s.M34:f4}");
        w.WriteStringValue($"{s.M41:f4} {s.M42:f4} {s.M43:f4} {s.M44:f4}");
        w.WriteEndArray();
    }
}

#endregion
