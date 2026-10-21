using OpenTK.Graphics.OpenGL;
using OpenX.Gfx.OpenGL;
using System;
using System.Numerics;

namespace GameX.Engines.OpenGL;

/// <summary>
/// One mesh in clip space. Positions are centered and scaled, then tilted so Y-up reads as a three-quarter view.
/// </summary>
public sealed class GlMeshDraw : IGlDrawable {
    readonly Vector3[] _vertices;
    readonly int[] _indices;
    int _vao;
    int _count;
    bool _ready;

    public GlMeshDraw(Vector3[] vertices, int[] indices) {
        _vertices = vertices ?? [];
        _indices = indices ?? [];
    }

    public void DrawGl() {
        if (!_ready) Upload();
        if (_vao == 0 || _count <= 0) return;
        GL.UseProgram(Program());
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _count, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    void Upload() {
        _ready = true;
        if (_vertices.Length == 0 || _indices.Length == 0) return;
        var min = _vertices[0];
        var max = _vertices[0];
        for (var i = 1; i < _vertices.Length; i++) {
            var p = _vertices[i];
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        var center = (min + max) * 0.5f;
        var ext = Vector3.Max(max - center, center - min);
        var scale = Math.Max(ext.X, Math.Max(ext.Y, ext.Z));
        if (scale < 1e-6f) scale = 1f;
        scale = 0.8f / scale;
        var packed = new float[_vertices.Length * 3];
        for (var i = 0; i < _vertices.Length; i++) {
            var p = (_vertices[i] - center) * scale;
            packed[i * 3] = p.X * 0.9f + p.Z * 0.35f;
            packed[i * 3 + 1] = p.Y * 0.9f - p.Z * 0.2f;
            packed[i * 3 + 2] = 0f;
        }
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        var vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, packed.Length * sizeof(float), packed, BufferUsage.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, 0);
        var ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(int), _indices, BufferUsage.StaticDraw);
        GL.BindVertexArray(0);
        _count = _indices.Length;
    }

    static int _program;
    static int Program() {
        if (_program != 0) return _program;
        var vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, "#version 330 core\nlayout(location=0) in vec3 a;\nvoid main(){ gl_Position = vec4(a, 1.0); }\n");
        GL.CompileShader(vs);
        var fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, "#version 330 core\nout vec4 c;\nvoid main(){ c = vec4(0.75, 0.78, 0.82, 1.0); }\n");
        GL.CompileShader(fs);
        var program = GL.CreateProgram();
        GL.AttachShader(program, vs);
        GL.AttachShader(program, fs);
        GL.LinkProgram(program);
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        return _program = program;
    }
}
