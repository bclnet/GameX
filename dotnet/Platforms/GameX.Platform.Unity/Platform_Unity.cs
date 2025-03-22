﻿using OpenStack.Gfx;
using OpenStack.Gfx.Textures;
using OpenStack.Sfx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static OpenStack.Gfx.Textures.TextureFormat;
using Debug = OpenStack.Debug;
using Rendering = UnityEngine.Rendering;
using Shader = UnityEngine.Shader;
using TextureFormat = UnityEngine.TextureFormat;

namespace GameX.Platforms;

// UnityExtensions
public static class UnityExtensions
{
    public static UnityEngine.Experimental.Rendering.GraphicsFormat ToUnity(this DXGI_FORMAT source) => (UnityEngine.Experimental.Rendering.GraphicsFormat)source;
    //public static UnityEngine.TextureFormat ToUnity(this TextureUnityFormat source) => (UnityEngine.TextureFormat)source;

    // NifUtils
    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 source) { MathX.Swap(ref source.Y, ref source.Z); return new UnityEngine.Vector3(source.X, source.Y, source.Z); }
    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 source, float meterInUnits) => source.ToUnity() / meterInUnits;
    public static UnityEngine.Matrix4x4 ToUnityRotationMatrix(this System.Numerics.Matrix4x4 rotationMatrix) => new UnityEngine.Matrix4x4
    {
        m00 = rotationMatrix.M11,
        m01 = rotationMatrix.M13,
        m02 = rotationMatrix.M12,
        m03 = 0,
        m10 = rotationMatrix.M31,
        m11 = rotationMatrix.M33,
        m12 = rotationMatrix.M32,
        m13 = 0,
        m20 = rotationMatrix.M21,
        m21 = rotationMatrix.M23,
        m22 = rotationMatrix.M22,
        m23 = 0,
        m30 = 0,
        m31 = 0,
        m32 = 0,
        m33 = 1
    };
    public static UnityEngine.Quaternion ToUnityQuaternionAsRotationMatrix(this System.Numerics.Matrix4x4 rotationMatrix) => ToQuaternionAsRotationMatrix(rotationMatrix.ToUnityRotationMatrix());
    public static UnityEngine.Quaternion ToQuaternionAsRotationMatrix(this UnityEngine.Matrix4x4 rotationMatrix) => UnityEngine.Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));
    public static UnityEngine.Quaternion ToUnityQuaternionAsEulerAngles(this System.Numerics.Vector3 eulerAngles)
    {
        var newEulerAngles = eulerAngles.ToUnity();
        var xRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.x, UnityEngine.Vector3.right);
        var yRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.y, UnityEngine.Vector3.up);
        var zRot = UnityEngine.Quaternion.AngleAxis(UnityEngine.Mathf.Rad2Deg * newEulerAngles.z, UnityEngine.Vector3.forward);
        return xRot * zRot * yRot;
    }
}

// UnityObjectBuilder
// MISSING

// UnityShaderBuilder
public class UnityShaderBuilder : ShaderBuilderBase<Shader>
{
    public override Shader CreateShader(object path, IDictionary<string, bool> args = null) => Shader.Find((string)path);
}

// UnityTextureBuilder
public class UnityTextureBuilder : TextureBuilderBase<Texture2D>
{
    Texture2D _defaultTexture;
    public override Texture2D DefaultTexture => _defaultTexture ??= CreateDefaultTexture();

    public void Release()
    {
        if (_defaultTexture != null) { UnityEngine.Object.Destroy(_defaultTexture); _defaultTexture = null; }
    }

    Texture2D CreateDefaultTexture() => new(4, 4);

    public override Texture2D CreateTexture(Texture2D reuse, ITexture source, Range? range = null)
    {
        var (bytes, fmt, _) = source.Begin("UN");
        try
        {
            if (bytes == null) return DefaultTexture;
            else if (fmt is ValueTuple<OpenStack.Gfx.Textures.TextureFormat, TexturePixel> z)
            {
                var (format, pixel) = z;
                var s = (pixel & TexturePixel.Signed) != 0;
                var f = (pixel & TexturePixel.Float) != 0;
                var textureFormat = format switch
                {
                    DXT1 => TextureFormat.DXT1,
                    DXT1A => default,
                    DXT3 => default,
                    DXT5 => TextureFormat.DXT5,
                    BC4 => TextureFormat.BC4,
                    BC5 => TextureFormat.BC5,
                    BC6H => TextureFormat.BC6H,
                    BC7 => TextureFormat.BC7,
                    ETC2 => TextureFormat.ETC2_RGB,
                    ETC2_EAC => TextureFormat.ETC2_RGBA8,
                    //
                    I8 => default,
                    L8 => default,
                    R8 => TextureFormat.R8,
                    R16 => f ? TextureFormat.RFloat : s ? TextureFormat.R16_SIGNED : TextureFormat.R16,
                    RG16 => f ? TextureFormat.RGFloat : s ? TextureFormat.RG16_SIGNED : TextureFormat.RG16,
                    RGB24 => f ? default : s ? TextureFormat.RGB24_SIGNED : TextureFormat.RGB24,
                    RGB565 => TextureFormat.RGB565,
                    RGBA32 => f ? TextureFormat.RGBAFloat : s ? TextureFormat.RGBA32_SIGNED : TextureFormat.RGBA32,
                    ARGB32 => TextureFormat.ARGB32,
                    BGRA32 => TextureFormat.BGRA32,
                    BGRA1555 => default,
                    _ => throw new ArgumentOutOfRangeException("TextureFormat", $"{format}")
                };
                if (format == DXT3)
                {
                    textureFormat = TextureFormat.DXT5;
                    TextureConvert.Dxt3ToDtx5(bytes, source.Width, source.Height, source.MipMaps);
                }
                var tex = new Texture2D(source.Width, source.Height, textureFormat, source.MipMaps, false);
                tex.LoadRawTextureData(bytes);
                tex.Apply();
                tex.Compress(true);
                return tex;
            }
            else throw new ArgumentOutOfRangeException(nameof(fmt), $"{fmt}");
        }
        finally { source.End(); }
    }

    public override Texture2D CreateSolidTexture(int width, int height, float[] rgba) => new Texture2D(width, height);

    public override Texture2D CreateNormalMap(Texture2D texture, float strength)
    {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);
        float xLeft, xRight, yUp, yDown, yDelta, xDelta;
        var normalTexture = new Texture2D(texture.width, texture.height, UnityEngine.TextureFormat.ARGB32, true);
        for (var y = 0; y < normalTexture.height; y++)
            for (var x = 0; x < normalTexture.width; x++)
            {
                xLeft = texture.GetPixel(x - 1, y).grayscale * strength;
                xRight = texture.GetPixel(x + 1, y).grayscale * strength;
                yUp = texture.GetPixel(x, y - 1).grayscale * strength;
                yDown = texture.GetPixel(x, y + 1).grayscale * strength;
                xDelta = (xLeft - xRight + 1) * 0.5f;
                yDelta = (yUp - yDown + 1) * 0.5f;
                normalTexture.SetPixel(x, y, new UnityEngine.Color(xDelta, yDelta, 1.0f, yDelta));
            }
        normalTexture.Apply();
        return normalTexture;
    }

    public override void DeleteTexture(Texture2D texture) => UnityEngine.Object.Destroy(texture);
}

// UnityMaterialBuilder
#region UnityMaterialBuilder

/// <summary>
/// A material that uses the new Standard Shader.
/// </summary>
public class UnityMaterialBuilder_Standard : MaterialBuilderBase<Material, Texture2D>
{
    Material _defaultMaterial;
    Material _standardMaterial;
    Material _standardCutoutMaterial;

    public UnityMaterialBuilder_Standard(TextureManager<Texture2D> textureManager) : base(textureManager)
    {
        _standardMaterial = new Material(Shader.Find("Standard"));
        _standardCutoutMaterial = UnityEngine.Resources.Load<Material>("Materials/StandardCutout");
        _defaultMaterial = BuildMaterial();
    }

    public override Material DefaultMaterial => _defaultMaterial;

    public override Material CreateMaterial(object key)
    {
        switch (key)
        {
            case null: return BuildMaterial();
            case IFixedMaterial p:
                Material material;
                if (p.AlphaBlended) material = BuildMaterialBlended((Rendering.BlendMode)p.SrcBlendMode, (Rendering.BlendMode)p.DstBlendMode);
                else if (p.AlphaTest) material = BuildMaterialTested(p.AlphaCutoff);
                else material = BuildMaterial();
                if (p.MainFilePath != null)
                {
                    (material.mainTexture, _) = TextureManager.CreateTexture(p.MainFilePath);
                    if (NormalGeneratorIntensity != null)
                    {
                        material.EnableKeyword("_NORMALMAP");
                        material.SetTexture("_BumpMap", TextureManager.CreateNormalMap((Texture2D)material.mainTexture, NormalGeneratorIntensity.Value));
                    }
                }
                else material.DisableKeyword("_NORMALMAP");
                if (p.BumpFilePath != null)
                {
                    material.EnableKeyword("_NORMALMAP");
                    material.SetTexture("_NORMALMAP", TextureManager.CreateTexture(p.BumpFilePath).tex);
                }
                return material;
            case MaterialTerrain _: return BuildMaterialTerrain();
            default: throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    Material BuildMaterial()
    {
        var material = new Material(Shader.Find("Standard"));
        material.CopyPropertiesFromMaterial(_standardMaterial);
        return material;
    }

    static Material BuildMaterialTerrain() => new(Shader.Find("Nature/Terrain/Diffuse"));

    Material BuildMaterialBlended(Rendering.BlendMode srcBlendMode, Rendering.BlendMode dstBlendMode)
    {
        var material = BuildMaterialTested();
        //material.SetInt("_SrcBlend", (int)srcBlendMode);
        //material.SetInt("_DstBlend", (int)dstBlendMode);
        return material;
    }

    Material BuildMaterialTested(float cutoff = 0.5f)
    {
        var material = new Material(Shader.Find("Standard"));
        material.CopyPropertiesFromMaterial(_standardCutoutMaterial);
        material.SetFloat("_Cutout", cutoff);
        return material;
    }
}

/// <summary>
/// A material that uses the default shader created for TESUnity.
/// </summary>
public class UnityMaterialBuilder_Default(TextureManager<Texture2D> textureManager) : MaterialBuilderBase<Material, Texture2D>(textureManager)
{
    static readonly Material _defaultMaterial = BuildMaterial();

    public override Material DefaultMaterial => _defaultMaterial;

    public override Material CreateMaterial(object key)
    {
        switch (key)
        {
            case null: return BuildMaterial();
            case IFixedMaterial p:
                Material material;
                if (p.AlphaBlended) material = BuildMaterialBlended((Rendering.BlendMode)p.SrcBlendMode, (Rendering.BlendMode)p.DstBlendMode);
                else if (p.AlphaTest) material = BuildMaterialTested(p.AlphaCutoff);
                else material = BuildMaterial();
                if (p.MainFilePath != null && material.HasProperty("_MainTex")) material.SetTexture("_MainTex", TextureManager.CreateTexture(p.MainFilePath).tex);
                if (p.DetailFilePath != null && material.HasProperty("_DetailTex")) material.SetTexture("_DetailTex", TextureManager.CreateTexture(p.DetailFilePath).tex);
                if (p.DarkFilePath != null && material.HasProperty("_DarkTex")) material.SetTexture("_DarkTex", TextureManager.CreateTexture(p.DarkFilePath).tex);
                if (p.GlossFilePath != null && material.HasProperty("_GlossTex")) material.SetTexture("_GlossTex", TextureManager.CreateTexture(p.GlossFilePath).tex);
                if (p.GlowFilePath != null && material.HasProperty("_Glowtex")) material.SetTexture("_Glowtex", TextureManager.CreateTexture(p.GlowFilePath).tex);
                if (p.BumpFilePath != null && material.HasProperty("_BumpTex")) material.SetTexture("_BumpTex", TextureManager.CreateTexture(p.BumpFilePath).tex);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
                return material;
            case MaterialTerrain _: return BuildMaterialTerrain();
            default: throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    static Material BuildMaterial() => new(Shader.Find("TES Unity/Standard"));

    static Material BuildMaterialTerrain() => new(Shader.Find("Nature/Terrain/Diffuse"));

    static Material BuildMaterialBlended(Rendering.BlendMode sourceBlendMode, Rendering.BlendMode destinationBlendMode)
    {
        var material = new Material(Shader.Find("TES Unity/Alpha Blended"));
        material.SetInt("_SrcBlend", (int)sourceBlendMode);
        material.SetInt("_DstBlend", (int)destinationBlendMode);
        return material;
    }

    static Material BuildMaterialTested(float cutoff = 0.5f)
    {
        var material = new Material(Shader.Find("TES Unity/Alpha Tested"));
        material.SetFloat("_Cutoff", cutoff);
        return material;
    }
}

/// <summary>
/// A material that uses the Unlit Shader.
/// </summary>
public class UnityMaterial_Unlite(TextureManager<Texture2D> textureManager) : MaterialBuilderBase<Material, Texture2D>(textureManager)
{
    static readonly Material _defaultMaterial = BuildMaterial();

    public override Material DefaultMaterial => _defaultMaterial;

    public override Material CreateMaterial(object key)
    {
        switch (key)
        {
            case null: return BuildMaterial();
            case IFixedMaterial p:
                Material material;
                if (p.AlphaBlended) material = BuildMaterialBlended((Rendering.BlendMode)p.SrcBlendMode, (Rendering.BlendMode)p.DstBlendMode);
                else if (p.AlphaTest) material = BuildMaterialTested(p.AlphaCutoff);
                else material = BuildMaterial();
                if (p.MainFilePath != null) (material.mainTexture, _) = TextureManager.CreateTexture(p.MainFilePath);
                return material;
            case MaterialTerrain _: return BuildMaterialTerrain();
            default: throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    static Material BuildMaterial() => new(Shader.Find("Unlit/Texture"));

    static Material BuildMaterialTerrain() => new(Shader.Find("Nature/Terrain/Diffuse"));

    static Material BuildMaterialBlended(Rendering.BlendMode sourceBlendMode, Rendering.BlendMode destinationBlendMode)
    {
        var material = BuildMaterialTested();
        material.SetInt("_SrcBlend", (int)sourceBlendMode);
        material.SetInt("_DstBlend", (int)destinationBlendMode);
        return material;
    }

    static Material BuildMaterialTested(float cutoff = 0.5f)
    {
        var material = new Material(Shader.Find("Unlit/Transparent Cutout"));
        material.SetFloat("_AlphaCutoff", cutoff);
        return material;
    }
}

/// <summary>
/// A material that uses the legacy Bumped Diffuse Shader.
/// </summary>
public class UnityMaterialBuilder_BumpedDiffuse(ITextureManager<Texture2D> textureManager) : MaterialBuilderBase<Material, Texture2D>(textureManager)
{
    static readonly Material _defaultMaterial = BuildMaterial();

    public override Material DefaultMaterial => _defaultMaterial;

    public override Material CreateMaterial(object key)
    {
        switch (key)
        {
            case null: return BuildMaterial();
            case IFixedMaterial p:
                Material material;
                if (p.AlphaBlended) material = BuildMaterialBlended((Rendering.BlendMode)p.SrcBlendMode, (Rendering.BlendMode)p.DstBlendMode);
                else if (p.AlphaTest) material = BuildMaterialTested(p.AlphaCutoff);
                else material = BuildMaterial();
                if (p.MainFilePath != null)
                {
                    (material.mainTexture, _) = TextureManager.CreateTexture(p.MainFilePath);
                    if (NormalGeneratorIntensity != null) material.SetTexture("_BumpMap", TextureManager.CreateNormalMap((Texture2D)material.mainTexture, NormalGeneratorIntensity.Value));
                }
                if (p.BumpFilePath != null) material.SetTexture("_BumpMap", TextureManager.CreateTexture(p.BumpFilePath).tex);
                return material;
            case MaterialTerrain _: return BuildMaterialTerrain();
            default: throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    static Material BuildMaterial() => new(Shader.Find("Legacy Shaders/Bumped Diffuse"));

    static Material BuildMaterialTerrain() => new(Shader.Find("Nature/Terrain/Diffuse"));

    static Material BuildMaterialBlended(Rendering.BlendMode srcBlendMode, Rendering.BlendMode dstBlendMode)
    {
        var material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Bumped Diffuse"));
        material.SetInt("_SrcBlend", (int)srcBlendMode);
        material.SetInt("_DstBlend", (int)dstBlendMode);
        return material;
    }

    static Material BuildMaterialTested(float cutoff = 0.5f)
    {
        var material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Bumped Diffuse"));
        material.SetFloat("_AlphaCutoff", cutoff);
        return material;
    }
}

#endregion

// IUnityGfx
public interface IUnityGfx : IOpenGfxAny<GameObject, Material, Texture2D, Shader> { }

// UnityGfx
public class UnityGfx : IUnityGfx
{
    readonly PakFile _source;
    readonly ITextureManager<Texture2D> _textureManager;
    readonly IMaterialManager<Material, Texture2D> _materialManager;
    readonly IObjectManager<GameObject, Material, Texture2D> _objectManager;
    readonly IShaderManager<Shader> _shaderManager;

    public UnityGfx(PakFile source)
    {
        _source = source;
        _textureManager = new TextureManager<Texture2D>(source, new UnityTextureBuilder());
        //switch (MaterialType.Default)
        //{
        //    case MaterialType.None: _material = null; break;
        //    case MaterialType.Default: _material = new DefaultMaterial(_textureManager); break;
        //    case MaterialType.Standard: _material = new StandardMaterial(_textureManager); break;
        //    case MaterialType.Unlit: _material = new UnliteMaterial(_textureManager); break;
        //    default: _material = new BumpedDiffuseMaterial(_textureManager); break;
        //}
        _materialManager = new MaterialManager<Material, Texture2D>(source, _textureManager, new UnityMaterialBuilder_BumpedDiffuse(_textureManager));
        //_objectManager = new ObjectManager<GameObject, Material, Texture2D>(source, _materialManager, new UnityObjectBuilder(0));
        _shaderManager = new ShaderManager<Shader>(source, new UnityShaderBuilder());
    }

    public PakFile Source => _source;
    public ITextureManager<Texture2D> TextureManager => _textureManager;
    public IMaterialManager<Material, Texture2D> MaterialManager => _materialManager;
    public IObjectManager<GameObject, Material, Texture2D> ObjectManager => _objectManager;
    public IShaderManager<Shader> ShaderManager => _shaderManager;
    public Texture2D CreateTexture(object path, Range? level = null) => _textureManager.CreateTexture(path, level).tex;
    public void PreloadTexture(object path) => _textureManager.PreloadTexture(path);
    public GameObject CreateObject(object path) => _objectManager.CreateObject(path).obj;
    public void PreloadObject(object path) => _objectManager.PreloadObject(path);
    public Shader CreateShader(object path, IDictionary<string, bool> args = null) => _shaderManager.CreateShader(path, args).sha;

    public Task<T> LoadFileObject<T>(object path) => _source.LoadFileObject<T>(path);
}

// UnitySfx
public class UnitySfx(PakFile source) : SystemSfx(source)
{
}

// UnityPlatform
public class UnityPlatform : Platform
{
    public static readonly Platform This = new UnityPlatform();
    UnityPlatform() : base("UN", "Unity")
    {
        var task = Task.Run(Application.platform.ToString);
        try
        {
            Tag = task.Result;
            //Debug.Log(Tag);
            GfxFactory = source => new UnityGfx(source);
            SfxFactory = source => new UnitySfx(source);
            AssertFunc = x => UnityEngine.Debug.Assert(x);
            LogFunc = UnityEngine.Debug.Log;
            LogFormatFunc = UnityEngine.Debug.LogFormat;
        }
        catch { Enabled = false; }
    }

    public override unsafe void Activate()
    {
        base.Activate();
        UnsafeX.Memcpy = (dest, src, count) => UnsafeUtility.MemCpy(dest, src, count);
    }

    public override unsafe void Deactivate()
    {
        base.Deactivate();
        UnsafeX.Memcpy = null;
    }
}

// UnityShellPlatform
public class UnityShellPlatform : Platform
{
    public static readonly Platform This = new UnityShellPlatform();
    UnityShellPlatform() : base("UN", "Unity") { }
}