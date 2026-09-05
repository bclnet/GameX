// PORT-SOURCE: Core/GameX/Transforms/UnknownTransform.cs
// PORT-SHA: e6a95fbe28938834
// PORT-STATUS: done
//
// NOT PORTED — both members are stubs:
//
//     public static bool CanTransformAsset(...) => false;
//     public static Task<IUnknownFileModel> TransformAsset(...)
//         => throw new NotImplementedException();
//
// `CanTransformAsset` returns a constant `false`, so `TransformAsset` is
// unreachable through any caller that checks first — which is the same
// arrangement as `Platform_Test` in the OpenStack port: a guard that always
// declines in front of a body that always throws.
//
// When this is implemented, the shape it produces is
// `formats::i_unknown::UnknownFileModel`, which is ported.
//
// Kept as a file so the 1:1 mapping holds and `sync-check.sh` tracks drift.
