#!/usr/bin/env python3
"""
Builds a Rust workspace that mirrors the .NET solution 1:1 and emits PORT_MAP.tsv,
the manifest that makes parallel C#/Rust updates tractable.
"""
import hashlib, os, re, subprocess, sys
from pathlib import Path

SRC = Path("/home/claude/src/dotnet")
OUT = Path("/home/claude/rust")
SKIP = {"bin", "obj", ".vs", "packages", "node_modules"}

# .NET project name -> (workspace-relative crate dir, crate name)
CRATES = {
    "OpenX.PolyIO":           ("core/polyio",            "openx-poly"),
    "OpenX.Polyfills":        ("core/polyfills",         "openx-poly"),
    "OpenX":                  ("core/openx",         "openx"),
    "OpenX.Aix":              ("aix/aix",                "openx-aix"),
    "OpenX.AixTests":         ("aix/aix-tests",          "openx-aix-tests"),
    "OpenX.Gfx":              ("gfx/gfx",                "openx-gfx"),
    "OpenX.Gfx.Egin":         ("gfx/gfx-egin",           "openx-gfx-egin"),
    "OpenX.Gfx.Other":        ("gfx/gfx-other",          "openx-gfx-other"),
    "OpenX.GfxTests":         ("gfx/gfx-tests",          "openx-gfx-tests"),
    "OpenX.Phy":              ("phy/phy",                "openx-phy"),
    "OpenX.Phy2":             ("phy/phy2",               "openx-phy2"),
    "OpenX.PhyTests":         ("phy/phy-tests",          "openx-phy-tests"),
    "OpenX.Sfx":              ("sfx/sfx",                "openx-sfx"),
    "OpenX.Sfx.Al":           ("sfx/sfx-al",             "openx-sfx-al"),
    "OpenX.Sfx.Ogg":          ("sfx/sfx-ogg",            "openx-sfx-ogg"),
    "OpenX.SfxTests":         ("sfx/sfx-tests",          "openx-sfx-tests"),
    "OpenX.Vfx":              ("vfx/vfx",                "openx-vfx"),
    "OpenX.Vfx.Program":      ("vfx/vfx-program",        "openx-vfx-program"),
    "OpenX.Engine.EginX":   ("engines/eginx",        "openx-engine-eginx"),
    "OpenX.Engine.Godot":   ("engines/godot",        "openx-engine-godot"),
    "OpenX.Engine.Mg":      ("engines/mg",           "openx-engine-mg"),
    "OpenX.Engine.O3de":    ("engines/o3de",         "openx-engine-o3de"),
    "OpenX.Engine.Ogre":    ("engines/ogre",         "openx-engine-ogre"),
    "OpenX.Engine.OpenGL":  ("engines/opengl",       "openx-engine-opengl"),
    "OpenX.Engine.Sdl":     ("engines/sdl",          "openx-engine-sdl"),
    "OpenX.Engine.Stride":  ("engines/stride",       "openx-engine-stride"),
    "OpenX.Engine.Tests":   ("engines/tests",        "openx-engine-tests"),
    "OpenX.Engine.Unity":   ("engines/unity",        "openx-engine-unity"),
    "OpenX.Engine.Unreal":  ("engines/unreal",       "openx-engine-unreal"),
    "OpenX.Engine.Vk":      ("engines/vk",           "openx-engine-vk"),
    "OpenX.Wpf.Control":      ("engines/wpf-control",  "openx-wpf-control"),
    "OpenX.Wpf.Stride":       ("engines/wpf-stride",   "openx-wpf-stride"),
}

RUST_KEYWORDS = {
    "as","box","break","const","continue","crate","dyn","else","enum","extern","false","fn","for",
    "if","impl","in","let","loop","match","mod","move","mut","pub","ref","return","self","static",
    "struct","super","trait","true","type","unsafe","use","where","while","async","await","try",
    "abstract","become","final","macro","override","priv","typeof","unsized","virtual","yield",
}


def snake(name: str) -> str:
    """PascalCase / dotted / plus-joined C# name -> rust snake_case module ident."""
    name = name.replace("+", "_").replace(".", "_").replace("-", "_").replace(" ", "_")
    name = re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", name)
    name = re.sub(r"(?<=[A-Z])(?=[A-Z][a-z])", "_", name)
    name = re.sub(r"_+", "_", name).strip("_").lower()
    if not name:
        name = "m"
    if name[0].isdigit():
        name = "m_" + name
    if name in RUST_KEYWORDS:
        name = name + "_"
    return name


def find_projects():
    """Return {project_name: project_dir} for every non-build .csproj."""
    projects = {}
    for csproj in SRC.rglob("*.csproj"):
        if SKIP & set(csproj.parts):
            continue
        projects[csproj.stem] = csproj.parent
    return projects


def cs_files(proj_dir: Path):
    for f in sorted(proj_dir.rglob("*.cs")):
        if SKIP & set(f.parts):
            continue
        # generated assembly-info style files carry no portable logic
        if f.name.startswith(".NETStandard") or f.name.endswith("AssemblyInfo.cs"):
            continue
        yield f


# folder names that would collide with Rust's special module files
DIR_REMAP = {"lib": "vendor", "main": "main_", "mod": "mod_"}


def rust_path_for(rel: Path) -> str:
    """dotnet-relative path inside a project -> rust src-relative path."""
    parts = [DIR_REMAP.get(snake(p), snake(p)) for p in rel.parts[:-1]]
    stem = snake(rel.stem)
    if stem in ("lib", "main", "mod"):
        stem += "_"
    return "/".join(parts + [stem + ".rs"])


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()[:16]


def loc(path: Path) -> int:
    try:
        return len(path.read_text(encoding="utf-8-sig", errors="replace").splitlines())
    except Exception:
        return 0


def main():
    projects = find_projects()
    unknown = sorted(set(projects) - set(CRATES))
    if unknown:
        print("WARNING unmapped projects:", unknown, file=sys.stderr)

    rows = []
    # crate dir -> set of rust module paths, for mod-tree generation
    modules = {}

    for proj_name, proj_dir in sorted(projects.items()):
        if proj_name not in CRATES:
            continue
        crate_dir, crate_name = CRATES[proj_name]
        modules.setdefault(crate_dir, set())
        for f in cs_files(proj_dir):
            rel = f.relative_to(proj_dir)
            rpath = rust_path_for(rel)
            modules[crate_dir].add(rpath)
            rows.append({
                "crate": crate_name,
                "crate_dir": crate_dir,
                "cs": str(f.relative_to(SRC)),
                "rs": f"{crate_dir}/src/{rpath}",
                "loc": loc(f),
                "sha": sha(f),
                "status": "todo",
            })

    OUT.mkdir(parents=True, exist_ok=True)

    # ---- PORT_MAP.tsv -------------------------------------------------------
    with (OUT / "PORT_MAP.tsv").open("w") as fh:
        fh.write("status\tcs_loc\tcs_sha256_16\tcs_path\trs_path\tcrate\n")
        for r in sorted(rows, key=lambda r: (r["crate_dir"], r["cs"])):
            fh.write(f'{r["status"]}\t{r["loc"]}\t{r["sha"]}\t{r["cs"]}\t{r["rs"]}\t{r["crate"]}\n')

    # ---- workspace Cargo.toml ----------------------------------------------
    members = "\n".join(f'    "{d}",' for d in sorted(modules))
    (OUT / "Cargo.toml").write_text(
        "[workspace]\n"
        'resolver = "2"\n'
        "members = [\n" + members + "\n]\n\n"
        "[workspace.package]\n"
        'edition = "2021"\n'
        'rust-version = "1.75"\n'
        'license = "MIT"\n\n'
        "[workspace.dependencies]\n"
        "bytemuck = { version = \"1\", features = [\"derive\"] }\n"
        "byteorder = \"1\"\n"
        "glam = { version = \"0.29\", features = [\"bytemuck\"] }\n"
        "half = \"2\"\n"
        "thiserror = \"1\"\n"
        "bitflags = \"2\"\n"
        "\n[profile.release]\n"
        "lto = true\n"
        "codegen-units = 1\n"
    )

    # ---- per-crate skeletons ------------------------------------------------
    proj_deps = {}
    for proj_name, proj_dir in projects.items():
        csproj = proj_dir / f"{proj_name}.csproj"
        text = csproj.read_text(encoding="utf-8-sig", errors="replace") if csproj.exists() else ""
        refs = re.findall(r'ProjectReference\s+Include="([^"]+)"', text)
        deps = []
        for ref in refs:
            stem = Path(ref.replace("\\", "/")).stem
            if stem in CRATES:
                deps.append(CRATES[stem])
        proj_deps[proj_name] = deps

    for proj_name, (crate_dir, crate_name) in CRATES.items():
        if proj_name not in projects:
            continue
        cdir = OUT / crate_dir
        (cdir / "src").mkdir(parents=True, exist_ok=True)
        depth = len(Path(crate_dir).parts)
        up = "../" * depth
        dep_lines = "".join(
            f'{dn} = {{ path = "{up}{dd}" }}\n' for dd, dn in proj_deps.get(proj_name, [])
        )
        (cdir / "Cargo.toml").write_text(
            "[package]\n"
            f'name = "{crate_name}"\n'
            'version = "0.1.0"\n'
            "edition.workspace = true\n"
            "rust-version.workspace = true\n"
            "license.workspace = true\n\n"
            "[dependencies]\n" + dep_lines
        )

        # build the nested mod tree from the rust file paths
        tree = {}
        for rpath in sorted(modules[crate_dir]):
            node = tree
            parts = rpath[:-3].split("/")
            for p in parts[:-1]:
                node = node.setdefault(p, {})
            node.setdefault(parts[-1], None)

        def emit(node, dirpath: Path, indent: int) -> str:
            out = []
            pad = "" if indent == 0 else ""
            for key in sorted(node):
                child = node[key]
                if child is None:
                    out.append(f"{pad}pub mod {key};")
                else:
                    out.append(f"{pad}pub mod {key};")
                    sub = dirpath / key
                    sub.mkdir(parents=True, exist_ok=True)
                    (sub / "mod.rs").write_text(
                        f"// mirrors dotnet folder `{key}` — see PORT_MAP.tsv\n"
                        + emit(child, sub, indent + 1) + "\n"
                    )
            return "\n".join(out)

        body = emit(tree, cdir / "src", 0)
        (cdir / "src" / "lib.rs").write_text(
            f"//! `{crate_name}` — 1:1 port of .NET project `{proj_name}`.\n"
            "//!\n"
            "//! Module layout mirrors the C# folder/file layout exactly so the two trees\n"
            "//! can be diffed and updated in parallel. See PORT_MAP.tsv at the workspace root.\n\n"
            + body + "\n"
        )

    # ---- stub every mapped .rs that does not exist yet ----------------------
    created = 0
    for r in rows:
        p = OUT / r["rs"]
        p.parent.mkdir(parents=True, exist_ok=True)
        if p.exists():
            continue
        p.write_text(
            f"// PORT-SOURCE: {r['cs']}\n"
            f"// PORT-SHA: {r['sha']}\n"
            f"// PORT-STATUS: todo ({r['loc']} LOC in C#)\n"
            "//\n"
            "// Not yet ported. Keep this header in sync when porting: update PORT-SHA to the\n"
            "// C# file's current hash and flip PORT-STATUS to `done`. `./sync-check.sh` reports\n"
            "// any file whose C# source has changed since the port.\n"
        )
        created += 1

    tot = sum(r["loc"] for r in rows)
    print(f"crates: {len(modules)}  files mapped: {len(rows)}  C# LOC: {tot}  stubs created: {created}")


if __name__ == "__main__":
    main()
