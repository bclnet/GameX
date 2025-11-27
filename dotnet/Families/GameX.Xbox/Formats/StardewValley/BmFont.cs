using GameX.Xbox.Formats.Xna;
using System;

namespace GameX.Xbox.Formats.StardewValley.BmFont;

// Font
[RType("BmFont.XmlSourceReader"), RAssembly("BmFont")] class XmlSourceReader : TypeReader<string> { public override string Read(ContentReader r, string o) => r.ReadString(); }
