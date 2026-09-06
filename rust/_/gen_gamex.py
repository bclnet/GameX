#!/usr/bin/env python3
"""Build a Rust workspace mirroring the GameX solution 1:1, and emit PORT_MAP.tsv.

Same approach as the OpenStack port's gen_port.py, but the crate map is derived
from the project tree rather than hand-listed - there are 60 projects and the
naming is regular.

GameX references OpenStack as a sibling checkout (..\\..\\..\\OpenStack\\dotnet\\...),
so those turn into path dependencies on the crates from the earlier port.
"""
import csv
import hashlib
import re
import sys
from pathlib import Path

SRC = Path("/home/claude/src/gamex/dotnet")
OUT = Path("/home/claude/gamex-rust")
# Where the ported OpenStack crates live, relative to a GameX crate directory.
OPENSTACK_REL = "../../../rust"
SKIP = {"bin", "obj", ".vs", "packages", "node_modules"}

RUST_KEYWORDS = {
    "as", "box", "break", "const", "continue", "crate", "dyn", "else", "enum", "extern",
    "false", "fn", "for", "if", "impl", "in", "let", "loop", "match", "mod", "move", "mut",
    "pub", "ref", "return", "self", "static", "struct", "super", "trait", "true", "type",
    "unsafe", "use", "where", "while", "async", "await", "try", "abstract", "become",
    "final", "macro", "override", "priv", "typeof", "unsized", "virtual", "yield",
}

# Folder names that collide with Rust's special module files.
DIR_REMAP = {"lib": "vendor", "main": "main_", "mod": "mod_"}

# The acronym heuristic below is right for HTTPServer -> http_server but wrong for
# names like FFmpeg. Kept as an explicit table, as in the OpenStack port.
NAME_OVERRIDES = {
    "FFmpegService": "ffmpeg_service",
    "FFmpeg": "ffmpeg",
}

# Which .NET project maps to which OpenStack crate, for dependency wiring.
OPENSTACK_CRATES = {
    "OpenStack": ("openstack", "core/openstack"),
    "OpenStack.Gfx": ("openstack-gfx", "gfx/gfx"),
    "OpenStack.Gfx.Egin": ("openstack-gfx-egin", "gfx/gfx-egin"),
    "OpenStack.Sfx": ("openstack-sfx", "sfx/sfx"),
    "OpenStack.Vfx": ("openstack-vfx", "vfx/vfx"),
    "OpenStack.PolyIO": ("openstack-polyio", "core/polyio"),
    "OpenStack.Polyfills": ("openstack-polyfills", "core/polyfills"),
    "OpenStack.Platform.EginX": ("openstack-platform-eginx", "platforms/eginx"),
    "OpenStack.Platform.Godot": ("openstack-platform-godot", "platforms/godot"),
    "OpenStack.Platform.Mg": ("openstack-platform-mg", "platforms/mg"),
    "OpenStack.Platform.O3de": ("openstack-platform-o3de", "platforms/o3de"),
    "OpenStack.Platform.Ogre": ("openstack-platform-ogre", "platforms/ogre"),
    "OpenStack.Platform.OpenGL": ("openstack-platform-opengl", "platforms/opengl"),
    "OpenStack.Platform.Sdl": ("openstack-platform-sdl", "platforms/sdl"),
    "OpenStack.Platform.Stride": ("openstack-platform-stride", "platforms/stride"),
    "OpenStack.Platform.Unity": ("openstack-platform-unity", "platforms/unity"),
    "OpenStack.Platform.Unreal": ("openstack-platform-unreal", "platforms/unreal"),
}


def snake(name: str) -> str:
    """PascalCase / dotted / plus-joined C# name -> rust snake_case ident."""
    if name in NAME_OVERRIDES:
        return NAME_OVERRIDES[name]
    # A trailing `+` is C#'s convention for a partial-class continuation
    # (Entity.cs / Entity+.cs). Stripping it collapses the two onto one path, so
    # it becomes an explicit suffix. Interior `+` (Polyfill+BinaryReader.cs)
    # already reads as a separator.
    while name.endswith("+"):
        name = name[:-1] + "_plus"
    name = name.replace("+", "_").replace(".", "_").replace("-", "_").replace(" ", "_")
    name = re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", name)
    name = re.sub(r"(?<=[A-Z])(?=[A-Z][a-z])", "_", name)
    name = re.sub(r"_+", "_", name).strip("_").lower()
    if not name:
        name = "m"
    if name[0].isdigit():
        name = "m_" + name
    if name in RUST_KEYWORDS:
        name += "_"
    return name


def crate_for(project: str) -> tuple[str, str]:
    """GameX project name -> (crate name, workspace-relative directory)."""
    # GameX.Bethesda.Platform -> gamex-bethesda-platform
    parts = project.split(".")
    crate = "-".join(snake(p).replace("_", "") if p != "GameX" else "gamex" for p in parts)
    crate = crate.replace("_", "-")
    if project == "GameX":
        return "gamex", "core/gamex"
    if project == "GameXTests":
        return "gamex-tests", "core/gamex-tests"
    tail = "-".join(c for c in crate.split("-")[1:])
    # Group by the solution folder the project actually lives in.
    return crate, f"{{group}}/{tail}"


def find_projects() -> dict[str, Path]:
    out = {}
    for csproj in SRC.rglob("*.csproj"):
        if SKIP & set(csproj.parts):
            continue
        out[csproj.stem] = csproj.parent
    return out


def cs_files(proj_dir: Path):
    for f in sorted(proj_dir.rglob("*.cs")):
        if SKIP & set(f.parts):
            continue
        if f.name.endswith("AssemblyInfo.cs") or f.name.startswith(".NETStandard"):
            continue
        yield f


def rust_path_for(rel: Path) -> str:
    parts = [DIR_REMAP.get(snake(p), snake(p)) for p in rel.parts[:-1]]
    stem = snake(rel.stem)
    if stem in ("lib", "main", "mod"):
        stem += "_"
    return "/".join(parts + [stem + ".rs"])


def live_loc(path: Path) -> int:
    try:
        lines = path.read_text(encoding="utf-8-sig", errors="replace").splitlines()
    except Exception:
        return 0
    return sum(1 for l in lines if l.strip() and not l.strip().startswith("//"))


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()[:16]


def main() -> int:
    projects = find_projects()
    # Solution folder each project sits in (Core / Families / Platforms / Applications).
    group_of = {}
    for name, d in projects.items():
        rel = d.relative_to(SRC)
        group_of[name] = snake(rel.parts[0]) if len(rel.parts) > 1 else "core"

    crates = {}
    for name in projects:
        crate, dirpat = crate_for(name)
        crates[name] = (crate, dirpat.replace("{group}", group_of[name]))

    rows = []
    modules: dict[str, set[str]] = {}
    for name, d in sorted(projects.items()):
        crate, cdir = crates[name]
        modules.setdefault(cdir, set())
        for f in cs_files(d):
            rel = f.relative_to(d)
            rpath = rust_path_for(rel)
            modules[cdir].add(rpath)
            rows.append({
                "status": "todo",
                "cs_loc": live_loc(f),
                "cs_sha256_16": sha(f),
                "cs_path": str(f.relative_to(SRC)),
                "rs_path": f"{cdir}/src/{rpath}",
                "crate": crate,
            })

    OUT.mkdir(parents=True, exist_ok=True)
    with (OUT / "PORT_MAP.tsv").open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=list(rows[0].keys()), delimiter="\t")
        w.writeheader()
        for r in sorted(rows, key=lambda r: (r["crate"], r["cs_path"])):
            w.writerow(r)

    members = "\n".join(f'    "{d}",' for d in sorted(modules))
    (OUT / "Cargo.toml").write_text(
        "[workspace]\n"
        'resolver = "2"\n'
        f"members = [\n{members}\n]\n\n"
        "[workspace.package]\n"
        'edition = "2021"\n'
        'rust-version = "1.75"\n'
        'license = "MIT"\n\n'
        "[workspace.dependencies]\n"
        'bytemuck = { version = "1", features = ["derive"] }\n'
        'glam = { version = "0.29", features = ["bytemuck"] }\n'
        'half = "2"\n'
        'bitflags = "2"\n'
        "\n[profile.release]\nlto = true\ncodegen-units = 1\n"
    )

    # Per-project dependency wiring, GameX-internal and OpenStack.
    for name, d in projects.items():
        crate, cdir = crates[name]
        csproj = d / f"{name}.csproj"
        text = csproj.read_text(encoding="utf-8-sig", errors="replace")
        deps = []
        for ref in re.findall(r'ProjectReference\s+Include="([^"]+)"', text):
            stem = Path(ref.replace("\\", "/")).stem
            if stem in crates:
                dc, dd = crates[stem]
                depth = len(Path(cdir).parts)
                deps.append(f'{dc} = {{ path = "{"../" * depth}{dd}" }}')
            elif stem in OPENSTACK_CRATES:
                oc, od = OPENSTACK_CRATES[stem]
                depth = len(Path(cdir).parts)
                deps.append(f'{oc} = {{ path = "{"../" * depth}{OPENSTACK_REL}/{od}" }}')
        cd = OUT / cdir
        (cd / "src").mkdir(parents=True, exist_ok=True)
        (cd / "Cargo.toml").write_text(
            "[package]\n"
            f'name = "{crate}"\n'
            'version = "0.1.0"\n'
            "edition.workspace = true\n"
            "rust-version.workspace = true\n"
            "license.workspace = true\n\n"
            "[dependencies]\n" + "\n".join(sorted(set(deps))) + ("\n" if deps else "")
        )

        # Nested mod tree from the rust file paths.
        #
        # A file and a folder may share a name (GameX.Origin has both
        # Encryption.cs and an Encryption/ folder). Rust 2018 allows that:
        # the parent declares `pub mod encryption;` once, and encryption.rs
        # declares its own submodules. So leaves and children are tracked
        # separately rather than in one dict.
        children: dict[str, set[str]] = {}
        leaves: set[str] = set()
        for rpath in sorted(modules[cdir]):
            segs = rpath[:-3].split("/")
            leaves.add("/".join(segs))
            for i in range(len(segs)):
                parent = "/".join(segs[:i])
                children.setdefault(parent, set()).add(segs[i])
                if i < len(segs) - 1:
                    # ensure intermediate dirs exist as keys
                    children.setdefault("/".join(segs[: i + 1]), set())

        def emit(prefix: str, dirpath: Path) -> str:
            decls = []
            for key in sorted(children.get(prefix, ())):
                decls.append(f"pub mod {key};")
                child_prefix = f"{prefix}/{key}" if prefix else key
                sub = children.get(child_prefix)
                if not sub:
                    continue
                subdir = dirpath / key
                subdir.mkdir(parents=True, exist_ok=True)
                inner = emit(child_prefix, subdir)
                if child_prefix in leaves:
                    # foo.rs exists *and* foo/ has children: the submodule
                    # declarations belong in foo.rs, not foo/mod.rs.
                    leaf = dirpath / f"{key}.rs"
                    header = (
                        f"// PORT-SOURCE: (see PORT_MAP.tsv)\n"
                        if not leaf.exists() else ""
                    )
                    existing = leaf.read_text() if leaf.exists() else header
                    leaf.write_text(
                        existing.rstrip("\n")
                        + "\n\n// Submodules: this file shares its name with a sibling folder,\n"
                        + "// which Rust 2018 allows - the children are declared here.\n"
                        + inner + "\n"
                    )
                else:
                    (subdir / "mod.rs").write_text(
                        f"// mirrors dotnet folder `{key}` — see PORT_MAP.tsv\n"
                        + inner + "\n"
                    )
            return "\n".join(decls)

        body = emit("", cd / "src")
        (cd / "src" / "lib.rs").write_text(
            f"//! `{crate}` — 1:1 port of .NET project `{name}`.\n"
            "//!\n"
            "//! Module layout mirrors the C# folder/file layout exactly so the two trees\n"
            "//! can be diffed and updated in parallel. See PORT_MAP.tsv and PORTING.md.\n\n"
            + body + "\n"
        )

    created = 0
    for r in rows:
        p = OUT / r["rs_path"]
        p.parent.mkdir(parents=True, exist_ok=True)
        if p.exists():
            continue
        p.write_text(
            f"// PORT-SOURCE: {r['cs_path']}\n"
            f"// PORT-SHA: {r['cs_sha256_16']}\n"
            f"// PORT-STATUS: todo ({r['cs_loc']} live LOC in C#)\n"
            "//\n"
            "// Not yet ported. When porting, update PORT-SHA to the C# file's current\n"
            "// hash and flip PORT-STATUS to `done`. ./sync-check.sh reports drift.\n"
        )
        created += 1

    print(f"crates: {len(modules)}  files mapped: {len(rows)}  "
          f"live LOC: {sum(r['cs_loc'] for r in rows)}  stubs: {created}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
