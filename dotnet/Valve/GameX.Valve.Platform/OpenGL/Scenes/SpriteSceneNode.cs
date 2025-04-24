using GameX.Valve.Formats;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Egin;
using OpenStack.Gfx.OpenGL;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.OpenGL.Scenes;

//was:Renderer/SpriteSceneNode
public class SpriteSceneNode : SceneNode {
    readonly int quadVao;

    readonly GLRenderMaterial material;
    readonly Shader shader;
    readonly Vector3 position;
    readonly float size;

    public SpriteSceneNode(Scene scene, Binary_Src resource, Vector3 position) : base(scene) {
        var gfx = scene.Gfx as OpenGLGfxModel;
        (material, _) = gfx.MaterialManager.CreateMaterial(resource);
        (shader, _) = gfx.ShaderManager.CreateShader(material.Material.ShaderName, material.Material.ShaderArgs);

        if (quadVao == 0) quadVao = SetupQuadBuffer();

        var paramMaterial = material.Material as MaterialShaderVProp;
        size = paramMaterial.FloatParams.GetValueOrDefault("g_flUniformPointSize", 16);

        this.position = position;
        var size3 = new Vector3(size);
        LocalBoundingBox = new AABB(position - size3, position + size3);
    }

    int SetupQuadBuffer() {
        GL.UseProgram(shader.Program);

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        var vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        var vertices = new[]
        {
            // position          ; texcoord
            -1.0f, -1.0f, 0.0f,  0.0f, 1.0f,
            -1.0f, 1.0f, 0.0f,   0.0f, 0.0f,
            1.0f, -1.0f, 0.0f,   1.0f, 1.0f,
            1.0f, 1.0f, 0.0f,    1.0f, 0.0f,
        };

        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        var attributes = new List<(string Name, int Size)>
        {
            ("vPOSITION", 3),
            ("vTEXCOORD", 2),
        };
        var stride = sizeof(float) * attributes.Sum(x => x.Size);
        var offset = 0;

        foreach (var (name, size) in attributes) {
            var attributeLocation = shader.GetAttribLocation(name);
            GL.EnableVertexAttribArray(attributeLocation);
            GL.VertexAttribPointer(attributeLocation, size, VertexAttribPointerType.Float, false, stride, offset);
            offset += sizeof(float) * size;
        }

        GL.BindVertexArray(0);

        return vao;
    }

    public override void Render(Scene.RenderContext context) {
        GL.UseProgram(shader.Program);
        GL.BindVertexArray(quadVao);

        var viewProjectionMatrix = context.Camera.ViewProjectionMatrix.ToOpenTK();
        var cameraPosition = context.Camera.Location;

        // Create billboarding rotation (always facing camera)
        Matrix4x4.Decompose(context.Camera.CameraViewMatrix, out _, out var modelViewRotation, out _);
        modelViewRotation = Quaternion.Inverse(modelViewRotation);
        var billboardMatrix = Matrix4x4.CreateFromQuaternion(modelViewRotation);

        var scaleMatrix = Matrix4x4.CreateScale(size);
        var translationMatrix = Matrix4x4.CreateTranslation(position.X, position.Y, position.Z);

        var test = billboardMatrix * scaleMatrix * translationMatrix;
        var test2 = test.ToOpenTK();

        GL.UniformMatrix4(shader.GetUniformLocation("uProjectionViewMatrix"), false, ref viewProjectionMatrix);

        var transformTk = Transform;
        GL.UniformMatrix4(shader.GetUniformLocation("transform"), false, ref test2);

        material.Render(shader);

        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        material.PostRender();

        GL.Enable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public override void Update(Scene.UpdateContext context) { }
}
