using GameX.Formats;
using GameX.Formats.IUnknown;
using System;
using System.Threading.Tasks;

namespace GameX.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    public static bool CanTransformAsset(Archive left, Archive right, object source) => false;
    public static Task<IUnknownFileModel> TransformAsset(Archive left, Archive right, object source) => throw new NotImplementedException();
}