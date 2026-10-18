// Links native/diligent_shim.cpp. Default: no GAMEX_WITH_DILIGENT, no Diligent libs.
fn main() {
    let mut build = cc::Build::new();
    build
        .cpp(true)
        .file("native/diligent_shim.cpp")
        .include("native");
    if std::env::var_os("CARGO_FEATURE_DILIGENT").is_some() {
        build.define("GAMEX_WITH_DILIGENT", None);
    }
    build.compile("diligent_shim");
}
