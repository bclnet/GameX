using GameX.Crytek.Formats;
using GameX.Formats.Unknown;
using System.Threading.Tasks;

namespace GameX.Crytek.Transforms;

/// <summary>
/// UnknownTransform
/// </summary>
public static class UnknownTransform {
    internal static bool CanTransformFileObject(Archive left, Archive right, object source) => source is CryFile;
    internal static Task<IUnknownFileModel> TransformFileObjectAsync(Archive left, Archive right, object source) => Task.FromResult((IUnknownFileModel)source);
}