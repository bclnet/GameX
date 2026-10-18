// PORT-SOURCE: Engines/GameX.Engine.Unreal/Unreal_Render.cs
// PORT-SHA: 891999e8434805e4
// PORT-STATUS: todo (28 live LOC in C#)
//
// Not yet ported. When porting, update PORT-SHA to the C# file's current
// hash and flip PORT-STATUS to `done`. ./sync-check.sh reports drift.
// `unrealLinked` reports the OpenX native shim. It is not `UnrealRenderer.CreateRenderer`.

/// False on this build. Forwards to the OpenX C++ shim, which does not host Unreal.
pub fn unrealLinked() -> bool {
    openx_engine_unreal::unreal_shim::linked()
}
