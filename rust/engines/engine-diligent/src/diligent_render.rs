// PORT-SOURCE: Engines/GameX.Engine.Diligent/Diligent_Render.cs
// PORT-STATUS: todo
//
// Not ported. `DiligentRenderer.CreateRenderer` switches on TestTri and Texture.
// `diligentLinked` reports the OpenX native shim. It is not `DiligentRenderer.CreateRenderer`.

/// False on this build. Forwards to the OpenX C shim, which does not host Diligent.
pub fn diligentLinked() -> bool {
    openx_engine_diligent::diligent_shim::linked()
}

/// This build returns "not linked".
pub fn diligentVersion() -> &'static str {
    openx_engine_diligent::diligent_shim::version()
}
