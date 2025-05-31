using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameX.DesSer;

/// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to
#region DesSer

public interface IDesSer {
}

public static class DesSerExtensions {
    public static JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    static DesSerExtensions() {
        Options.Converters.Add(new Vector3JsonConverter());
        Options.Converters.Add(new Matrix4x4JsonConverter());
    }

    public static string Serialize<T>(this T source) => JsonSerializer.Serialize(source, Options);
}

public class Vector3JsonConverter : JsonConverter<Vector3> {
    public override Vector3 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Vector3 s, JsonSerializerOptions options) => w.WriteStringValue($"{s.X} {s.Y} {s.Z}");
}

public class Matrix4x4JsonConverter : JsonConverter<Matrix4x4> {
    public override Matrix4x4 Read(ref Utf8JsonReader r, Type s, JsonSerializerOptions options) => throw new NotImplementedException();
    public override void Write(Utf8JsonWriter w, Matrix4x4 s, JsonSerializerOptions options) {
        w.WriteStartArray();
        w.WriteStringValue($"{s.M11} {s.M12} {s.M13} {s.M14}");
        w.WriteStringValue($"{s.M21} {s.M22} {s.M23} {s.M24}");
        w.WriteStringValue($"{s.M31} {s.M32} {s.M33} {s.M34}");
        w.WriteStringValue($"{s.M41} {s.M42} {s.M43} {s.M44}");
        w.WriteEndArray();
    }
}



#endregion
