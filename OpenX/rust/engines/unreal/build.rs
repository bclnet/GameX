// Links native/unreal_shim.cpp. Default: no GAMEX_WITH_UNREAL, no Unreal libs.
fn main() {
    let mut build = cc::Build::new();
    build
        .cpp(true)
        .file("native/unreal_shim.cpp")
        .include("native");
    if std::env::var_os("CARGO_FEATURE_UNREAL").is_some() {
        build.define("GAMEX_WITH_UNREAL", None);
    }
    build.compile("unreal_shim");
}
