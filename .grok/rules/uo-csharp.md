# Ultima Online stays C# until it is merged

The Ultima Online client is being ported to C# only. Do not keep Python or Rust in sync for that work until the owner says it is fully merged into GameX.

This covers `dotnet/Families/GameX.Origin.UO`, `dotnet/Families/GameX.Origin.Netcode`, `dotnet/Families/GameX.Origin.Mg`, `dotnet/Families/GameX.Origin/Clients/UO`, and MonoGame host changes that exist only to run that client. Do not add or update `python/gamex/families/GameX_Origin_UO.py`, `Origin_UO/`, or `rust/families/origin-uo` for this port. Do not add `gamex-origin-uo` to the Rust workspace, and do not add `PORT_MAP` rows for files that exist only because of this port.

ADR 0001 still applies to every other change, including Origin resource parsers that already have twins. When the owner says the client is merged, those three languages come back into step for the code that remains.

ClassicUO's `Microsoft.Xna.Framework` types are MonoGame (`MonoGame.Framework.DesktopGL` 3.8.6). They compile only in `GameX.Origin.Mg`. `GameX.Origin.UO` is netstandard2.1 and does not reference that package. A draw stays a `GumpArt.Sink` call, and `UoGumpArt` in the Mg project is the `SpriteBatch`. Do not drop a ClassicUO draw because the source mentions XNA.

`GameX.Origin.UO/UI/Controls`, `UI/Gumps`, `UI/Gumps/Login`, and `UI/Gumps/CharCreation` match ClassicUO's `Game/UI` folders. The namespace stays `GameX.Origin.UO`.
