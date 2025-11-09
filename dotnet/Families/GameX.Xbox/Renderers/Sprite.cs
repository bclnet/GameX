using GameX.Eng;
using GameX.Xbox.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GameX.Xbox.Renderers;

public class SpriteFont<Texture2D>(Texture2D texture, List<Rectangle> glyph, List<Rectangle> cropping, List<char> characters, int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter) {
    public List<char> Characters = characters;
    public char? DefaultCharacter = defaultCharacter;
    public int LineSpacing = lineSpacing;
    public float Spacing = spacing;
    public Texture2D Texture = texture;
    public List<Rectangle> GlyphData = glyph;
    public List<Rectangle> CroppingData = cropping;
    public List<Vector3> Kerning = kerning;

    public unsafe static SpriteFont<Texture2D> Create(GraphicsDevice device, Binary_Xnb resource) {
        return null;
        //using (var ms = new MemoryStream(resource.ToArray()))
        //using (var r = new BinReader(ms)) {
        //    r.ReadByte();
        //    r.ReadByte();
        //    r.ReadByte();
        //    var c = r.ReadChar();
        //    var version = r.ReadByte();
        //    var flags = r.ReadByte();
        //    var compressed = (flags & 0x80) != 0;
        //    if (version != 5 && version != 4) throw new Exception("Invalid XNB version");
        //    var xnbLength = r.ReadInt32();

        //    var numberOfReaders = r.Read7BitEncodedInt();
        //    for (var i = 0; i < numberOfReaders; i++) {
        //        var originalReaderTypeString = r.ReadString();
        //        r.ReadInt32();
        //    }
        //    var shared = r.Read7BitEncodedInt();
        //    var typeReaderIndex = r.Read7BitEncodedInt();
        //    r.Read7BitEncodedInt();
        //    var format = (SurfaceFormat)r.ReadInt32();
        //    var width = r.ReadInt32();
        //    var height = r.ReadInt32();
        //    var levelCount = r.ReadInt32();
        //    var levelDataSizeInBytes = r.ReadInt32();
        //    byte[] levelData = null; // Don't assign this quite yet...
        //    int levelWidth = width >> 0;
        //    int levelHeight = height >> 0;
        //    levelData = r.ReadBytes(levelDataSizeInBytes);
        //    if (format != SurfaceFormat.Color) {
        //        levelData = DecompressDxt3(levelData, levelWidth, levelHeight);
        //        levelDataSizeInBytes = levelData.Length;
        //    }
        //    var texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);

        //    fixed (byte* ptr = levelData) texture.SetDataPointerEXT(0, null, (IntPtr)ptr, levelDataSizeInBytes);
        //    // glyphs
        //    r.Read7BitEncodedInt();
        //    var glyphCount = r.ReadInt32();
        //    var glyphs = new List<Rectangle>(glyphCount);
        //    for (var i = 0; i < glyphCount; i++) glyphs.Add(new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));

        //    // croppings
        //    r.Read7BitEncodedInt();
        //    int croppingCount = r.ReadInt32();
        //    var croppings = new List<Rectangle>(croppingCount);
        //    for (var i = 0; i < croppingCount; i++) croppings.Add(new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));

        //    // charMap
        //    r.Read7BitEncodedInt();
        //    var charCount = r.ReadInt32();
        //    var charMap = new List<char>(charCount);
        //    for (var i = 0; i < charCount; i++) charMap.Add(r.ReadChar());

        //    var lineSpacing = r.ReadInt32();
        //    var spacing = r.ReadSingle();

        //    // kernings
        //    r.Read7BitEncodedInt();
        //    int kerningCount = r.ReadInt32();
        //    var kernings = new List<Vector3>(croppingCount);
        //    for (var i = 0; i < kerningCount; i++)
        //        kernings.Add(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));

        //    // defaultChar
        //    var defaultChar = r.ReadBoolean() ? (char?)r.ReadChar() : null;
        //    return new SpriteFont(texture, glyphs, croppings, charMap, lineSpacing, spacing, kernings, defaultChar);
        //}
    }

    public Vector2 MeasureString(ReadOnlySpan<char> text) {
        if (text == null) throw new ArgumentNullException("text");
        if (text.Length == 0) return Vector2.Zero;
        var result = Vector2.Zero;
        float curLineWidth = 0.0f, finalLineHeight = LineSpacing;
        var firstInLine = true;
        foreach (char c in text) {
            // Special characters
            if (c == '\r') continue;
            if (c == '\n') {
                result.X = Math.Max(result.X, curLineWidth);
                result.Y += LineSpacing;
                curLineWidth = 0.0f;
                finalLineHeight = LineSpacing;
                firstInLine = true;
                continue;
            }

            // Get the List index from the character map, defaulting to the DefaultCharacter if it's set.
            var index = Characters.IndexOf(c);
            if (index == -1) index = !DefaultCharacter.HasValue ? Characters.IndexOf('?') : Characters.IndexOf(DefaultCharacter.Value);

            // For the first character in a line, always push the width rightward, even if the kerning pushes the character to the left.
            var cKern = Kerning[index];
            if (firstInLine) { curLineWidth += Math.Abs(cKern.X); firstInLine = false; }
            else curLineWidth += Spacing + cKern.X;

            // Add the character width and right-side bearing to the line width.
            curLineWidth += cKern.Y + cKern.Z;

            // If a character is taller than the default line height, increase the height to that of the line's tallest character.
            var cCropHeight = CroppingData[index].Height;
            if (cCropHeight > finalLineHeight) finalLineHeight = cCropHeight;
        }

        // Calculate the final width/height of the text box
        result.X = Math.Max(result.X, curLineWidth);
        result.Y += finalLineHeight;
        return result;
    }
}