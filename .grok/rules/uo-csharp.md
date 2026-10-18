# Ultima Online stays C# until it is merged

The Ultima Online client is being ported to C# only. Do not keep Python or Rust in sync for that work until the owner says it is fully merged into GameX.

This covers `dotnet/Families/GameX.Origin.UO`, `dotnet/Families/GameX.Origin/Clients/UO`, and MonoGame host changes that exist only to run that client. Do not add or update `python/gamex/families/GameX_Origin_UO.py`, `Origin_UO/`, or `rust/families/origin-uo` for this port. Do not add `gamex-origin-uo` to the Rust workspace, and do not add `PORT_MAP` rows for files that exist only because of this port.

ADR 0001 still applies to every other change, including Origin resource parsers that already have twins. When the owner says the client is merged, those three languages come back into step for the code that remains.
