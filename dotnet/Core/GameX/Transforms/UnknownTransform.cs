using GameX.Formats.Unknown;
using System;
using System.Threading.Tasks;

namespace GameX.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    public static bool CanTransformFileObject(Archive left, Archive right, object source) => false;
    public static Task<IUnknownFileModel> TransformFileObjectAsync(Archive left, Archive right, object source) => throw new NotImplementedException();
}