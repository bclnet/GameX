using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameX;

/// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to
#region DesSer

public static class DesSer {
    readonly static JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        IndentSize = 2,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    static DesSer() {
        Options.Converters.Add(new Color3JsonConverter());
        Options.Converters.Add(new ByteColor3JsonConverter());
        Options.Converters.Add(new Color4JsonConverter());
        Options.Converters.Add(new ByteColor4JsonConverter());
        Options.Converters.Add(new Vector2JsonConverter());
        Options.Converters.Add(new Vector3JsonConverter());
        Options.Converters.Add(new Vector4JsonConverter());
        Options.Converters.Add(new Matrix3x3JsonConverter());
        Options.Converters.Add(new Matrix4x4JsonConverter());
    }

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

public class Color3JsonConverter : JsonConverter<Color3> {
    public override Color3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Color3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B}");
}

public class ByteColor3JsonConverter : JsonConverter<ByteColor3> {
    public override ByteColor3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, ByteColor3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B}");
}

public class Color4JsonConverter : JsonConverter<Color4> {
    public override Color4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Color4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B} {s.A}");
}

public class ByteColor4JsonConverter : JsonConverter<ByteColor4> {
    public override ByteColor4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, ByteColor4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.R} {s.G} {s.B} {s.A}");
}

class Vector2JsonConverter : JsonConverter<Vector2> {
    public override Vector2 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector2 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f} {s.Y:f}");
}

class Vector3JsonConverter : JsonConverter<Vector3> {
    public override Vector3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f} {s.Y:f} {s.Z:f}");
}

class Vector4JsonConverter : JsonConverter<Vector4> {
    public override Vector4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector4 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X:f} {s.Y:f} {s.Z:f} {s.W:f}");
}

class Matrix3x3JsonConverter : JsonConverter<Matrix3x3> {
    public override Matrix3x3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix3x3 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f} {s.M12:f} {s.M13:f}");
        w.WriteStringValue($"{s.M21:f} {s.M22:f} {s.M23:f}");
        w.WriteStringValue($"{s.M31:f} {s.M32:f} {s.M33:f}");
        w.WriteEndArray();
    }
}

class Matrix4x4JsonConverter : JsonConverter<Matrix4x4> {
    public override Matrix4x4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix4x4 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11:f} {s.M12:f} {s.M13:f} {s.M14:f}");
        w.WriteStringValue($"{s.M21:f} {s.M22:f} {s.M23:f} {s.M24:f}");
        w.WriteStringValue($"{s.M31:f} {s.M32:f} {s.M33:f} {s.M34:f}");
        w.WriteStringValue($"{s.M41:f} {s.M42:f} {s.M43:f} {s.M44:f}");
        w.WriteEndArray();
    }
}

#endregion
