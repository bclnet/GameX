using GameX.Formats.Unknown;
using System.Threading.Tasks;

namespace GameX.Cig.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    internal static bool CanTransformAsset(Archive left, Archive right, object source) => Crytek.Transforms.UnknownTransform.CanTransformAsset(left, right, source);
    internal static Task<IUnknownFileModel> TransformAsset(Archive left, Archive right, object source) => Crytek.Transforms.UnknownTransform.TransformAsset(left, right, source);
}