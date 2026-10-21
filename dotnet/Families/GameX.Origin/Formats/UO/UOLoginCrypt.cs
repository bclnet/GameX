using System;

namespace GameX.Origin.Formats.UO;

/// <summary>
/// Login-stream wrapper over EncryptionHelper. Exists only for the C# Ultima Online client; no Python or Rust twin.
/// </summary>
public sealed class UOLoginCrypt {
    readonly EncryptionHelper _helper;

    public UOLoginCrypt(ClientVersion version) => _helper = new EncryptionHelper(version);

    public void Initialize(uint seed) => _helper.Initialize(true, seed);

    public void Encrypt(Span<byte> data) => _helper.Encrypt(true, data, data, data.Length);
}
