using GameX.Crytek.Formats;
using GameX.Formats.Unknown;
using System.Threading.Tasks;

namespace GameX.Crytek.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    internal static bool CanTransformAsset(Archive left, Archive right, object source) => source is CryFile;
    internal static Task<IUnknownFileModel> TransformAsset(Archive left, Archive right, object source) => Task.FromResult((IUnknownFileModel)source);
}