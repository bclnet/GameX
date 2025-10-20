using GameX.Formats.Unknown;
using System;
using System.Threading.Tasks;

namespace GameX.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    public static bool CanTransformFileObject(PakFile left, PakFile right, object source) => false;
    public static Task<IUnknownFileModel> TransformFileObjectAsync(PakFile left, PakFile right, object source) => throw new NotImplementedException();
}