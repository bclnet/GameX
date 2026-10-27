using GameX.Origin.Formats.UO;
using System;

namespace GameX.Origin.Netcode.UO;

/// <summary>
/// Login-stream wrapper over EncryptionHelper. Exists only for the C# Ultima Online client; no Python or Rust twin.
/// </summary>
public sealed class UOLoginCrypt {
    readonly EncryptionHelper _helper;

    public UOLoginCrypt(ClientVersion version) => _helper = new EncryptionHelper(version);

    /// <summary>Shard has no login or game cipher. Packets stay clear and the game stream is Huffman only.</summary>
    public bool Plain { get; set; }

    public void Initialize(uint seed) {
        if (!Plain) _helper.Initialize(true, seed);
    }

    public void Encrypt(Span<byte> data) {
        if (!Plain) _helper.Encrypt(true, data, data, data.Length);
    }

    /// <summary>Game cipher, after 0x5D. Same helper; its crypt objects are static.</summary>
    public void BeginGame(uint seed) {
        if (!Plain) _helper.Initialize(false, seed);
    }

    public void EncryptGame(Span<byte> data) {
        if (!Plain) _helper.Encrypt(false, data, data, data.Length);
    }

    public void DecryptGame(Span<byte> data) {
        if (!Plain) _helper.Decrypt(data, data, data.Length);
    }
}
