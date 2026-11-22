using OpenX;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mirage.Mcp;

/// <summary>
/// stdio MCP. One JSON object per line. Logs go to stderr so they stay off the protocol stream.
/// </summary>
static class Program {
    static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    static int Main(string[] args) {
        var stdout = Console.OpenStandardOutput();
        Console.SetOut(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Log.Func = m => Console.Error.WriteLine(m ?? "");
        _ = EngineX.Current;
        Log.Func = m => Console.Error.WriteLine(m ?? "");
        try {
            if (args.Length > 0 && args[0] == "review") return Review.Command(args);
            Serve(Console.OpenStandardInput(), stdout);
            return 0;
        }
        catch (Exception e) {
            Console.Error.WriteLine(e);
            return 1;
        }
    }

    static void Serve(Stream input, Stream output) {
        while (true) {
            var line = ReadMessage(input);
            if (line == null) return;
            JsonNode msg;
            try { msg = JsonNode.Parse(line); }
            catch (Exception e) { Console.Error.WriteLine(e.Message); continue; }
            var method = msg?["method"]?.GetValue<string>();
            var id = msg?["id"];
            if (id == null) continue;
            try {
                var result = method switch {
                    "initialize" => Initialize(msg),
                    "ping" => new JsonObject(),
                    "tools/list" => new JsonObject { ["tools"] = Tools() },
                    "tools/call" => Call(msg["params"] as JsonObject),
                    _ => null,
                };
                if (result == null) Write(output, Error(id, -32601, $"unknown method {method}"));
                else Write(output, new JsonObject { ["jsonrpc"] = "2.0", ["id"] = id.DeepClone(), ["result"] = result });
            }
            catch (Exception e) {
                Console.Error.WriteLine(e);
                Write(output, Error(id, -32603, e.Message));
            }
        }
    }

    static JsonObject Initialize(JsonNode msg) {
        var version = msg?["params"]?["protocolVersion"]?.GetValue<string>() ?? "2024-11-05";
        return new JsonObject {
            ["protocolVersion"] = version,
            ["capabilities"] = new JsonObject { ["tools"] = new JsonObject { ["listChanged"] = false } },
            ["serverInfo"] = new JsonObject { ["name"] = "mirage", ["version"] = "1" },
        };
    }

    static JsonObject Call(JsonObject parms) {
        var name = parms?["name"]?.GetValue<string>();
        var a = parms?["arguments"] as JsonObject;
        var result = name switch {
            "families" => Review.Families(),
            "games" => Review.Games(Str(a, "family")),
            "samples" => Review.Samples(Str(a, "family"), Str(a, "game")),
            "files" => Review.Files(Str(a, "family"), Str(a, "uri"), Str(a, "game"), Str(a, "filter"), Int(a, "limit", 40), Int(a, "offset", 0)),
            "open" => Review.Open(Str(a, "family"), Str(a, "uri"), Str(a, "game"), Str(a, "path"), Str(a, "type")),
            "texture" => Review.Texture(Str(a, "name")),
            _ => Review.Text(true, $"unknown tool {name}"),
        };
        return result;
    }

    static string Str(JsonObject a, string name) => a?[name]?.GetValue<string>();

    static int Int(JsonObject a, string name, int fallback) {
        var n = a?[name];
        if (n == null) return fallback;
        try { return n.GetValue<int>(); }
        catch { try { return (int)n.GetValue<long>(); } catch { return fallback; } }
    }

    static JsonArray Tools() => [
        Tool("families", "List every GameX family and how many of its games were found on this machine.", Schema()),
        Tool("games", "List the games in one family. found is the install path, or empty when the game is not on this machine.", Schema(("family", "string", "Family id, for example Arkane.", true))),
        Tool("samples", "List the sample files for one game. A path may be pak:inner. Pass one of those paths to open.", Schema(("family", "string", "Family id.", true), ("game", "string", "Game id, for example AF.", true))),
        Tool("files", "List paths in one archive. uri defaults to game:/#game. filter is a substring. limit defaults to 40.", Schema(("family", "string", "Family id.", true), ("game", "string", "Game id. Used when uri is omitted.", false), ("uri", "string", "Archive uri, for example game:/#AF or game:/data.pak#AF.", false), ("filter", "string", "Case-insensitive substring.", false), ("limit", "integer", "Page size, at most 200.", false), ("offset", "integer", "Number of matching paths to skip.", false))),
        Tool("open", "Open one asset through the Mirage engine and return the scene text, the resource list, and a PNG of the frame. path may be sample:N or pak:inner. The PNG is also written to the path named on the first line.", Schema(("family", "string", "Family id.", true), ("game", "string", "Game id. Used when uri is omitted.", false), ("uri", "string", "Archive uri. Defaults to game:/#game.", false), ("path", "string", "File path, pak:inner, or sample:N.", true), ("type", "string", "Texture, VideoTexture, Sprite, Object, Model, Level, Material, Audio, or TestTri. Omitted uses the sample type or the asset.", false))),
        Tool("texture", "Return one texture from the scene the last open built, when its pixels were kept.", Schema(("name", "string", "Texture name from the open resource list.", true))),
    ];

    static JsonObject Tool(string name, string description, JsonObject schema) => new() {
        ["name"] = name,
        ["description"] = description,
        ["inputSchema"] = schema,
    };

    static JsonObject Schema(params (string name, string type, string description, bool required)[] props) {
        var properties = new JsonObject();
        var required = new JsonArray();
        foreach (var p in props) {
            properties[p.name] = new JsonObject { ["type"] = p.type, ["description"] = p.description };
            if (p.required) required.Add(p.name);
        }
        var schema = new JsonObject { ["type"] = "object", ["properties"] = properties };
        if (required.Count > 0) schema["required"] = required;
        return schema;
    }

    static JsonObject Error(JsonNode id, int code, string message) => new() {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["error"] = new JsonObject { ["code"] = code, ["message"] = message },
    };

    static void Write(Stream output, JsonNode msg) {
        var bytes = Encoding.UTF8.GetBytes(msg.ToJsonString(Json));
        output.Write(bytes);
        output.WriteByte((byte)'\n');
        output.Flush();
    }

    static string ReadMessage(Stream input) {
        var line = ReadLine(input);
        if (line == null) return null;
        if (line.Length == 0) return ReadMessage(input);
        if (line[0] == '{') return line;
        var length = -1;
        while (line.Length > 0) {
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                int.TryParse(line.AsSpan(15).Trim(), out length);
            line = ReadLine(input);
            if (line == null) return null;
        }
        if (length < 0 || length > 8_000_000) return null;
        var buf = new byte[length];
        var off = 0;
        while (off < length) {
            var n = input.Read(buf, off, length - off);
            if (n <= 0) return null;
            off += n;
        }
        return Encoding.UTF8.GetString(buf);
    }

    static string ReadLine(Stream input) {
        var sb = new StringBuilder();
        int b;
        while ((b = input.ReadByte()) >= 0) {
            if (b == '\n') break;
            if (b != '\r') sb.Append((char)b);
        }
        if (b < 0 && sb.Length == 0) return null;
        return sb.ToString();
    }
}
