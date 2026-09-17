using System;
using System.Globalization;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Parses the standard id-software .map text format:
    ///   { "classname" "worldspawn" { (x y z)(x y z)(x y z) texture offX offY rot scaleX scaleY ... } }
    /// Does NOT support the Valve 220 format (explicit [ux uy uz off] UV axes) - if a map
    /// was exported in that format, parsing throws a clear NotSupportedException.
    /// </summary>
    public static class QuakeMapParser
    {
        public static QMap Parse(string text, QuakeParseOptions options)
        {
            var tokenizer = new Tokenizer(text);
            var map = new QMap();

            while (tokenizer.TryNext(out var token))
            {
                if (token == "{")
                {
                    map.Entities.Add(ParseEntity(tokenizer, options));
                }
                else
                {
                    throw new FormatException($"Unexpected token '{token}' at top level (expected '{{').");
                }
            }

            return map;
        }

        private static QEntity ParseEntity(Tokenizer tk, QuakeParseOptions options)
        {
            var entity = new QEntity();

            while (tk.TryNext(out var token))
            {
                if (token == "}")
                    break;

                if (token == "{")
                {
                    entity.Brushes.Add(ParseBrush(tk, options));
                    continue;
                }

                // Otherwise this token is a quoted key; the next token is its quoted value.
                if (!tk.TryNext(out var value))
                    throw new FormatException($"Expected a value for key \"{token}\" but reached end of file.");

                entity.Properties[token] = value;
            }

            entity.Classname = entity.Properties.TryGetValue("classname", out var cn) ? cn : "unknown";

            if (entity.Properties.TryGetValue("origin", out var originRaw))
                entity.Origin = ParseVector3(originRaw, options);

            return entity;
        }

        private static QBrush ParseBrush(Tokenizer tk, QuakeParseOptions options)
        {
            var brush = new QBrush();

            while (tk.TryNext(out var token))
            {
                if (token == "}")
                    break;

                if (token != "(")
                    throw new FormatException($"Expected '(' to start a brush face, got '{token}'.");

                brush.Faces.Add(ParseFace(tk, options));
            }

            return brush;
        }

        private static QFace ParseFace(Tokenizer tk, QuakeParseOptions options)
        {
            // The leading "(" for the first point was already consumed by ParseBrush.
            Vector3 p0 = ParsePointBody(tk, options);
            Expect(tk, "(");
            Vector3 p1 = ParsePointBody(tk, options);
            Expect(tk, "(");
            Vector3 p2 = ParsePointBody(tk, options);

            string texture = NextToken(tk);

            // Valve220 format puts "[" right after the texture name instead of a bare offset number.
            if (tk.TryPeek(out var maybeBracket) && maybeBracket == "[")
            {
                throw new NotSupportedException(
                    "This .map uses the Valve 220 UV format ([ux uy uz off] axes), which isn't " +
                    "supported yet. Re-export the map using the \"Standard\" / id-Tech format " +
                    "(in TrenchBroom: File > Export Map..., format = Standard).");
            }

            float offsetX = ParseFloatToken(tk);
            float offsetY = ParseFloatToken(tk);
            float rotation = ParseFloatToken(tk);
            float scaleX = ParseFloatToken(tk);
            float scaleY = ParseFloatToken(tk);

            // Some compiled/round-tripped maps append up to 3 extra integers
            // (content flags / surface flags / value). Swallow them if present.
            for (int i = 0; i < 3; i++)
            {
                if (!tk.TryPeek(out var maybeExtra)) break;
                if (!float.TryParse(maybeExtra, NumberStyles.Float, CultureInfo.InvariantCulture, out _)) break;
                tk.TryNext(out _); // consume it
            }

            return new QFace
            {
                Plane = QPlane.FromPoints(p0, p1, p2),
                Texture = texture,
                Offset = new Vector2(offsetX, offsetY),
                Rotation = rotation,
                Scale = new Vector2(scaleX == 0f ? 1f : scaleX, scaleY == 0f ? 1f : scaleY),
            };
        }

        private static Vector3 ParsePointBody(Tokenizer tk, QuakeParseOptions options)
        {
            float x = ParseFloatToken(tk);
            float y = ParseFloatToken(tk);
            float z = ParseFloatToken(tk);
            Expect(tk, ")");
            return QuakeCoordSpace.ConvertPoint(new Vector3(x, y, z), options);
        }

        private static Vector3 ParseVector3(string raw, QuakeParseOptions options)
        {
            var parts = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return Vector3.zero;

            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
            return QuakeCoordSpace.ConvertPoint(new Vector3(x, y, z), options);
        }

        private static void Expect(Tokenizer tk, string expected)
        {
            if (!tk.TryNext(out var token) || token != expected)
                throw new FormatException($"Expected '{expected}' but got '{token ?? "<eof>"}'.");
        }

        private static string NextToken(Tokenizer tk)
        {
            if (!tk.TryNext(out var token))
                throw new FormatException("Unexpected end of file.");
            return token;
        }

        private static float ParseFloatToken(Tokenizer tk)
        {
            string token = NextToken(tk);
            if (!float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Expected a number but got '{token}'.");
            return value;
        }

        /// <summary>
        /// Minimal hand-rolled lexer: punctuation ({, }, (, )) as single-char tokens,
        /// "quoted strings" with quotes stripped, and bare words/numbers otherwise.
        /// Supports // line comments and one token of lookahead.
        /// </summary>
        private class Tokenizer
        {
            private readonly string _text;
            private int _pos;
            private string _buffered;
            private bool _hasBuffered;

            public Tokenizer(string text)
            {
                _text = text;
                _pos = 0;
            }

            public bool TryNext(out string token)
            {
                if (_hasBuffered)
                {
                    token = _buffered;
                    _hasBuffered = false;
                    return true;
                }
                return TryReadRaw(out token);
            }

            public bool TryPeek(out string token)
            {
                if (!_hasBuffered)
                {
                    if (!TryReadRaw(out _buffered))
                    {
                        token = null;
                        return false;
                    }
                    _hasBuffered = true;
                }
                token = _buffered;
                return true;
            }

            private bool TryReadRaw(out string token)
            {
                SkipWhitespaceAndComments();

                if (_pos >= _text.Length)
                {
                    token = null;
                    return false;
                }

                char c = _text[_pos];

                if (c == '{' || c == '}' || c == '(' || c == ')' || c == '[' || c == ']')
                {
                    token = c.ToString();
                    _pos++;
                    return true;
                }

                if (c == '"')
                {
                    int start = ++_pos;
                    while (_pos < _text.Length && _text[_pos] != '"') _pos++;
                    token = _text.Substring(start, _pos - start);
                    if (_pos < _text.Length) _pos++; // skip closing quote
                    return true;
                }

                int s = _pos;
                while (_pos < _text.Length &&
                       !char.IsWhiteSpace(_text[_pos]) &&
                       "(){}[]\"".IndexOf(_text[_pos]) < 0)
                {
                    _pos++;
                }
                token = _text.Substring(s, _pos - s);
                return true;
            }

            private void SkipWhitespaceAndComments()
            {
                while (_pos < _text.Length)
                {
                    if (char.IsWhiteSpace(_text[_pos])) { _pos++; continue; }

                    if (_text[_pos] == '/' && _pos + 1 < _text.Length && _text[_pos + 1] == '/')
                    {
                        while (_pos < _text.Length && _text[_pos] != '\n') _pos++;
                        continue;
                    }

                    break;
                }
            }
        }
    }
}
