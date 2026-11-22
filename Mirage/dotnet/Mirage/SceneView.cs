using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mirage;

/// <summary>
/// SceneView paints the scene Mirage already built: kept texture pixels, sprites, and mesh bounds, into one image.
/// ToText() stays the form tests assert on.
/// </summary>
public static class SceneView {
    public static byte[] Png(SceneTree tree, int width = 768, int height = 512) {
        if (tree == null) throw new ArgumentNullException(nameof(tree));
        width = Math.Max(64, width); height = Math.Max(64, height);
        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(255, 28, 30, 36));
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        var pictures = Pictures(tree).Take(16).ToList();
        var meshes = Meshes(tree).ToList();
        var realMesh = meshes.Any(m => m.Mesh?.Name is not ("Quad" or "Triangle"));
        var caption = Math.Min(18, height / 5);
        var view = new Rectangle(0, 0, width, height - caption);
        if (pictures.Count > 0 && realMesh) {
            var split = Math.Max(120, view.Width * 55 / 100);
            DrawPictures(g, pictures, new Rectangle(view.X, view.Y, split, view.Height));
            DrawMeshes(g, meshes, new Rectangle(view.X + split, view.Y, view.Width - split, view.Height));
        }
        else if (pictures.Count > 0) DrawPictures(g, pictures, view);
        else DrawMeshes(g, meshes, view);
        using var font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        g.DrawString($"{pictures.Count} pictures, {meshes.Count} meshes", font, Brushes.Gainsboro, 6, height - caption);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    /// <summary>
    /// One texture as a PNG. Packed formats that are not raw pixels become a labelled swatch.
    /// </summary>
    public static byte[] TexturePng(Texture tex, int maxEdge = 512) {
        if (tex == null) throw new ArgumentNullException(nameof(tex));
        var w = Math.Clamp(tex.Width <= 0 ? 128 : tex.Width, 1, maxEdge);
        var h = Math.Clamp(tex.Height <= 0 ? 64 : tex.Height, 1, maxEdge);
        if (tex.Width > maxEdge || tex.Height > maxEdge) {
            var s = Math.Min(maxEdge / (float)Math.Max(tex.Width, 1), maxEdge / (float)Math.Max(tex.Height, 1));
            w = Math.Max(1, (int)(tex.Width * s)); h = Math.Max(1, (int)(tex.Height * s));
        }
        else if (tex.Width > 0 && tex.Width < 64 && tex.Height > 0) {
            var s = Math.Min(4, 128 / tex.Width);
            w = tex.Width * s; h = Math.Max(1, tex.Height * s);
        }
        using var bmp = new Bitmap(Math.Max(w, 64), Math.Max(h, 32) + 16, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(255, 28, 30, 36));
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        var dest = new Rectangle(0, 0, bmp.Width, bmp.Height - 16);
        if (!TryBlit(g, tex, dest)) {
            using var font = new Font(FontFamily.GenericSansSerif, 12f, FontStyle.Regular, GraphicsUnit.Pixel);
            g.DrawString($"{tex.Name}\n{tex.Width}x{tex.Height} {tex.Format}", font, Brushes.Gainsboro, new RectangleF(4, 4, dest.Width - 8, dest.Height - 8));
        }
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    static List<Texture> Pictures(SceneTree tree) {
        var seen = new HashSet<Texture>();
        var list = new List<Texture>();
        void Add(Texture t) { if (t != null && t.Width > 0 && t.Height > 0 && seen.Add(t)) list.Add(t); }
        foreach (var r in tree.Resources.All) {
            switch (r) {
                case Texture t: Add(t); break;
                case Sprite s: Add(s.Texture); break;
                case Material m: foreach (var kv in m.Textures) Add(kv.Value); break;
            }
        }
        return list;
    }

    static IEnumerable<MeshRenderer> Meshes(SceneTree tree) {
        foreach (var n in tree.Root.DescendantsAndSelf()) {
            if (!Shown(n)) continue;
            var mr = n.GetComponent<MeshRenderer>();
            if (mr?.Mesh != null) yield return mr;
        }
    }

    static bool Shown(Node n) {
        for (var p = n; p != null; p = p.Parent) if (!p.Visible) return false;
        return true;
    }

    static void DrawPictures(Graphics g, List<Texture> pictures, Rectangle area) {
        var n = pictures.Count;
        var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(n)));
        var rows = Math.Max(1, (int)Math.Ceiling(n / (double)cols));
        var cw = Math.Max(1, area.Width / cols);
        var ch = Math.Max(1, area.Height / rows);
        using var font = new Font(FontFamily.GenericSansSerif, 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        for (var i = 0; i < n; i++) {
            var cell = new Rectangle(area.X + (i % cols) * cw, area.Y + (i / cols) * ch, cw, ch);
            var dest = new Rectangle(cell.X + 2, cell.Y + 2, Math.Max(1, cell.Width - 4), Math.Max(1, cell.Height - 16));
            var tex = pictures[i];
            if (!TryBlit(g, tex, Fit(tex.Width, tex.Height, dest))) {
                g.FillRectangle(Brushes.DimGray, dest);
                g.DrawString($"{tex.Width}x{tex.Height} {tex.Format}", font, Brushes.White, dest);
            }
            g.DrawString(tex.Name ?? "", font, Brushes.Gainsboro, cell.X + 2, cell.Bottom - 14);
        }
    }

    static Rectangle Fit(int srcW, int srcH, Rectangle dest) {
        if (srcW <= 0 || srcH <= 0) return dest;
        var s = Math.Min(dest.Width / (float)srcW, dest.Height / (float)srcH);
        var w = Math.Max(1, (int)(srcW * s));
        var h = Math.Max(1, (int)(srcH * s));
        return new Rectangle(dest.X + (dest.Width - w) / 2, dest.Y + (dest.Height - h) / 2, w, h);
    }

    static void DrawMeshes(Graphics g, List<MeshRenderer> meshes, Rectangle area) {
        var pts = new List<Vector2>();
        var edges = new List<(int a, int b, int color)>();
        var colors = new[] { Color.FromArgb(220, 180, 190, 220), Color.FromArgb(220, 140, 190, 160), Color.FromArgb(220, 210, 170, 120), Color.FromArgb(220, 190, 140, 170) };
        foreach (var mr in meshes) {
            var mesh = mr.Mesh;
            var node = mr.Node;
            if (node == null) continue;
            var wm = node.WorldMatrix;
            var color = edges.Count == 0 ? 0 : (edges.Count / Math.Max(1, mesh.TriangleCount)) % colors.Length;
            if (mesh.Vertices != null && mesh.Vertices.Length > 0 && mesh.Vertices.Length <= 8000) {
                var b = pts.Count;
                foreach (var v in mesh.Vertices) { var w = Vector3.Transform(v, wm); pts.Add(new Vector2(w.X, w.Y)); }
                var tris = mesh.Triangles;
                if (tris != null)
                    for (var i = 0; i + 2 < tris.Length && edges.Count < 12000; i += 3) {
                        int a = tris[i], c = tris[i + 1], d = tris[i + 2];
                        if ((uint)a >= (uint)mesh.Vertices.Length || (uint)c >= (uint)mesh.Vertices.Length || (uint)d >= (uint)mesh.Vertices.Length) continue;
                        edges.Add((b + a, b + c, color)); edges.Add((b + c, b + d, color)); edges.Add((b + d, b + a, color));
                    }
            }
            else {
                var b = pts.Count;
                var lo = mesh.BoundsMin; var hi = mesh.BoundsMax;
                Vector3[] box = [new(lo.X, lo.Y, lo.Z), new(hi.X, lo.Y, lo.Z), new(hi.X, hi.Y, lo.Z), new(lo.X, hi.Y, lo.Z), new(lo.X, lo.Y, hi.Z), new(hi.X, lo.Y, hi.Z), new(hi.X, hi.Y, hi.Z), new(lo.X, hi.Y, hi.Z)];
                foreach (var v in box) { var w = Vector3.Transform(v, wm); pts.Add(new Vector2(w.X, w.Y)); }
                int[] e = [0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7];
                for (var i = 0; i < e.Length; i += 2) edges.Add((b + e[i], b + e[i + 1], color));
            }
        }
        if (pts.Count == 0) return;
        float minX = pts[0].X, maxX = pts[0].X, minY = pts[0].Y, maxY = pts[0].Y;
        foreach (var p in pts) { minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X); minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y); }
        var spanX = Math.Max(maxX - minX, 1e-4f);
        var spanY = Math.Max(maxY - minY, 1e-4f);
        var scale = Math.Min((area.Width - 8) / spanX, (area.Height - 8) / spanY);
        PointF Map(Vector2 p) => new(area.X + 4 + (p.X - minX) * scale, area.Bottom - 4 - (p.Y - minY) * scale);
        using var pen0 = new Pen(colors[0]);
        using var pen1 = new Pen(colors[1]);
        using var pen2 = new Pen(colors[2]);
        using var pen3 = new Pen(colors[3]);
        Pen[] pens = [pen0, pen1, pen2, pen3];
        foreach (var e in edges) g.DrawLine(pens[e.color % pens.Length], Map(pts[e.a]), Map(pts[e.b]));
    }

    static bool TryBlit(Graphics g, Texture tex, Rectangle dest) {
        if (!Rows(tex, out var bpp, out var stride, out var order)) return false;
        using var src = new Bitmap(tex.Width, tex.Height, PixelFormat.Format32bppArgb);
        var data = src.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try {
            var row = new byte[data.Stride];
            for (var y = 0; y < tex.Height; y++) {
                Array.Clear(row, 0, row.Length);
                var s = y * stride;
                for (var x = 0; x < tex.Width; x++) {
                    var i = s + x * bpp;
                    byte r, gv, b, a;
                    switch (order) {
                        case 0: r = tex.Pixels[i]; gv = tex.Pixels[i + 1]; b = tex.Pixels[i + 2]; a = tex.Pixels[i + 3]; break;
                        case 1: b = tex.Pixels[i]; gv = tex.Pixels[i + 1]; r = tex.Pixels[i + 2]; a = tex.Pixels[i + 3]; break;
                        case 2: a = tex.Pixels[i]; r = tex.Pixels[i + 1]; gv = tex.Pixels[i + 2]; b = tex.Pixels[i + 3]; break;
                        case 3: r = tex.Pixels[i]; gv = tex.Pixels[i + 1]; b = tex.Pixels[i + 2]; a = 255; break;
                        default: r = gv = b = tex.Pixels[i]; a = 255; break;
                    }
                    var o = x * 4;
                    row[o] = b; row[o + 1] = gv; row[o + 2] = r; row[o + 3] = a;
                }
                Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
            }
        }
        finally { src.UnlockBits(data); }
        g.DrawImage(src, dest);
        return true;
    }

    /// <summary>
    /// order 0 RGBA, 1 BGRA, 2 ARGB, 3 RGB, 4 gray. Stride is the source row step, including GDI padding.
    /// </summary>
    static bool Rows(Texture tex, out int bpp, out int stride, out int order) {
        bpp = 0; stride = 0; order = 0;
        if (tex?.Pixels == null || tex.Width <= 0 || tex.Height <= 0) return false;
        var head = (tex.Format ?? "").Split(':')[0];
        switch (head) {
            case "RGBA32": bpp = 4; order = 0; break;
            case "BGRA32": bpp = 4; order = 1; break;
            case "ARGB32": bpp = 4; order = 2; break;
            case "RGB24": bpp = 3; order = 3; break;
            case "L8": case "I8": case "R8": bpp = 1; order = 4; break;
            default: return false;
        }
        var packed = tex.Width * bpp;
        if (tex.Pixels.Length == tex.Height * packed) { stride = packed; return true; }
        if (tex.Height > 0 && tex.Pixels.Length % tex.Height == 0) {
            var s = tex.Pixels.Length / tex.Height;
            if (s >= packed) { stride = s; return true; }
        }
        var aligned = (packed + 3) & ~3;
        if (tex.Pixels.Length >= tex.Height * aligned) { stride = aligned; return true; }
        return false;
    }
}
