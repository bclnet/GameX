using OpenX.Client;
using OpenX.Gfx;
using OpenX.Gfx.Vk;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#pragma warning disable CS0649, CS0169

[assembly: InternalsVisibleTo("OpenX.GfxTests")]

namespace OpenX;

#region Client

/// <summary>
/// VkClientHost
/// </summary>
public class VkClientHost : IClientHost {
    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() => throw new NotImplementedException();
    /// <summary>
    /// Run
    /// </summary>
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Engine

/// <summary>
/// VkGfxApi
/// </summary>
public class VkGfxApi : IOpenGfxApi<object, object> {
    /// <summary>AddMeshCollider</summary>
    public void AddMeshCollider(object src, object mesh, bool isKinematic, bool isStatic) => throw new NotImplementedException();
    /// <summary>AddMeshRenderer</summary>
    public void AddMeshRenderer(object src, object mesh, object material, bool enabled, bool isStatic) => throw new NotImplementedException();
    /// <summary>AddMissingMeshCollidersRecursively</summary>
    public void AddMissingMeshCollidersRecursively(object src, bool isStatic) => throw new NotImplementedException();
    /// <summary>Attach</summary>
    public void Attach(GfxAttach method, object src, params object[] args) => throw new NotImplementedException();
    /// <summary>CreateMesh</summary>
    public object CreateMesh(object mesh) => throw new NotImplementedException();
    /// <summary>CreateObject</summary>
    public object CreateObject(string name, string tag = null, object parent = null) => throw new NotImplementedException();
    /// <summary>SetLayerRecursively</summary>
    public void SetLayerRecursively(object src, int layer) => throw new NotImplementedException();
    /// <summary>Parent</summary>
    public void Parent(object src, object parent) => throw new NotImplementedException();
    /// <summary>Transform</summary>
    public void Transform(object src, Vector3 position, Quaternion rotation, Vector3 localScale) => throw new NotImplementedException();
    /// <summary>Transform</summary>
    public void Transform(object src, Vector3 position, Matrix4x4 rotation, Vector3 localScale) => throw new NotImplementedException();
    /// <summary>SetVisible</summary>
    public void SetVisible(object src, bool visible) => throw new NotImplementedException();
    /// <summary>Destroy</summary>
    public void Destroy(object src) => throw new NotImplementedException();
}

/// <summary>
/// VkGfxSprite3D
/// </summary>
public class VkGfxSprite3D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    /// <summary>
    /// VkGfxSprite3D
    /// </summary>
    public VkGfxSprite3D() {
        //_spriteManager = new SpriteManager<Sprite2D>(source, new GodotSpriteBuilder());
    }

    /// <summary>SpriteManager</summary>
    public SpriteManager<object> SpriteManager => _spriteManager;
    /// <summary>Preload</summary>
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    /// <summary>Create</summary>
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// VkGfxModel
/// </summary>
public class VkGfxModel : IOpenGfxModel<object, object, object, object> {
    readonly MaterialManager<object, object> _materialManager;
    readonly ObjectModelManager<object, object, object> _objectManager;
    readonly ShaderManager<object> _shaderManager;
    readonly TextureManager<object> _textureManager;
    /// <summary>
    /// VkGfxModel
    /// </summary>
    public VkGfxModel() {
        //_textureManager = new TextureManager<object>(source, new VkTextureBuilder());
        //_materialManager = new MaterialManager<Material, int>(source, _textureManager, new GodotMaterialBuilder(_textureManager));
        //_objectManager = new ObjectManager<Model, Material, int>(source, _materialManager, new GodotObjectBuilder());
        //_shaderManager = new ShaderManager<int>(source, new GodotShaderBuilder());
    }

    /// <summary>MaterialManager</summary>
    public MaterialManager<object, object> MaterialManager => _materialManager;
    /// <summary>ObjectManager</summary>
    public ObjectModelManager<object, object, object> ObjectManager => _objectManager;
    /// <summary>ShaderManager</summary>
    public ShaderManager<object> ShaderManager => _shaderManager;
    /// <summary>TextureManager</summary>
    public TextureManager<object> TextureManager => _textureManager;
    /// <summary>Preload</summary>
    public void Preload(ISource source, object path) => throw new NotImplementedException();
    /// <summary>PreloadTexture</summary>
    public void PreloadTexture(ISource source, object path) => _textureManager.Preload(source, path);
    /// <summary>Create</summary>
    public Task<(object obj, object tag)> Create(ISource source, object path, bool isStatic, object parent = default) => throw new NotImplementedException();
    /// <summary>CreateShader</summary>
    public Task<(object sha, object tag)> CreateShader(ISource source, object path, Dictionary<string, bool> args = null) => throw new NotImplementedException();
    /// <summary>CreateTexture</summary>
    public Task<(object tex, object tag)> CreateTexture(ISource source, object path, System.Range? level = null) => _textureManager.Create(source, path, level);
    /// <summary>Post</summary>
    public void Post(object src, Vector3 position, Vector3 eulerAngles, float? scale, object parent) => throw new NotImplementedException();
}

/// <summary>
/// VkSfx
/// </summary>
public class VkSfx : SystemSfx { }

/// <summary>
/// VkEngine
/// </summary>
public class VkEngine : Engine {
    /// <summary>This</summary>
    public static readonly Engine This = new VkEngine();
    VkEngine() : base("VK", "Vulkan") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [new VkGfxApi(), null, new VkGfxSprite3D(), new VkGfxModel(), null, null];
        SfxFactory = () => [new VkSfx()];
    }

    /// <summary>
    /// Creates a Vulkan instance. No window and no swapchain.
    /// </summary>
    public static VkInstance CreateInstance(string applicationName = "GameX") => VkInstance.CreateInstance(applicationName);
}

/// <summary>
/// VkShellEngine
/// </summary>
public class VkShellEngine : Engine {
    /// <summary>This</summary>
    public static readonly Engine This = new VkShellEngine();
    VkShellEngine() : base("VK", "Vulkan") { }
}

#endregion
