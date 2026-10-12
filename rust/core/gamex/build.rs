// Embeds python/gamex/specs/*.json, as C# embeds them as `GameX.Specs.*` resources.
use std::{env, fs, path::Path};

fn main() {
    let specs = Path::new(env!("CARGO_MANIFEST_DIR")).join("../../../python/gamex/specs").canonicalize().unwrap();
    println!("cargo:rerun-if-changed={}", specs.display());
    let mut names: Vec<_> = fs::read_dir(&specs).unwrap().flatten().map(|e| e.file_name().into_string().unwrap()).filter(|s| s.ends_with(".json")).collect();
    names.sort();
    let mut out = String::from("pub static SPECS: &[(&str, &str)] = &[\n");
    for s in names { out += &format!("    ({s:?}, include_str!({:?})),\n", specs.join(&s)); }
    out += "];\n";
    fs::write(Path::new(&env::var("OUT_DIR").unwrap()).join("specs.rs"), out).unwrap();
}
