#!/usr/bin/env python3
"""Generate Rust from C# enum declarations.

46% of GameX is enum member tables — `Enums+Weenie.cs` alone is 14 enums with
31,108 member lines. Transcribing those by hand is how a one-digit typo ends up
mis-identifying a game asset months later, so they are generated.

What this handles:
  * explicit and implicit (auto-incrementing) discriminants
  * hex, negative, and character literals
  * `A = B` aliases and `A = B | C` composites, resolved against earlier members
  * [Flags] enums -> bitflags!, plain enums -> a Rust enum + from_raw()
  * duplicate discriminants (legal in C#, not in a Rust enum) -> the first
    becomes a variant, the rest become associated consts
  * XML doc comments and trailing // comments, carried across

Usage: gen_enums.py <Enums.cs> <out.rs> <PORT-SOURCE path>
"""
import re
import sys
from pathlib import Path

RUST_KEYWORDS = {
    "as", "break", "const", "continue", "crate", "else", "enum", "extern", "false", "fn",
    "for", "if", "impl", "in", "let", "loop", "match", "mod", "move", "mut", "pub", "ref",
    "return", "self", "static", "struct", "super", "trait", "true", "type", "unsafe",
    "use", "where", "while", "async", "await", "dyn", "abstract", "become", "box", "final",
    "macro", "override", "priv", "typeof", "unsized", "virtual", "yield", "try",
}

CS_TO_RUST = {
    "byte": "u8", "sbyte": "i8", "short": "i16", "ushort": "u16",
    "int": "i32", "uint": "u32", "long": "i64", "ulong": "u64",
}


def ident(name: str) -> str:
    """C# member name -> valid Rust ident, preserved verbatim where possible."""
    if name in RUST_KEYWORDS:
        return f"r#{name}"
    if name and name[0].isdigit():
        return "_" + name
    return name


def parse_enums(text: str):
    """Yield (name, base_type, is_flags, doc, [(member, expr, comment)])."""
    text = text.replace("\r\n", "\n")
    # Enum header, optionally preceded by attributes and/or an XML doc block.
    pattern = re.compile(
        r"(?P<doc>(?:^[ \t]*///.*\n)*)"
        # Attributes may sit on their own line(s) *or* inline before the
        # declaration: `[Flags] public enum DestFlag : byte { ... }` is legal
        # and appears seven times in Records.cs. Requiring `public enum` at
        # line start silently skipped all of them.
        r"(?P<attrs>(?:^[ \t]*\[[^\]]*\]\s*\n)*)"
        r"^[ \t]*(?P<inline>(?:\[[^\]]*\]\s*)*)public\s+enum\s+(?P<name>\w+)"
        r"(?:\s*:\s*(?P<base>\w+))?\s*\n?\s*\{",
        re.M,
    )
    # C# permits several nested enums with the same name in different classes
    # (Records.cs declares `Flag` nine times). Emitting them all at Rust module
    # scope collides, so each is qualified by its enclosing type.
    type_spans = []
    for tm in re.finditer(r"^[ \t]*(?:public|internal|private)?\s*"
                          r"(?:static\s+|sealed\s+|abstract\s+|partial\s+)*"
                          r"(?:class|struct|record)\s+(\w+)", text, re.M):
        i, depth, started = tm.end(), 0, False
        while i < len(text):
            if text[i] == "{":
                depth += 1
                started = True
            elif text[i] == "}":
                depth -= 1
                if started and depth == 0:
                    break
            i += 1
        type_spans.append((tm.start(), i, tm.group(1)))

    def enclosing(pos):
        """Innermost enclosing type name for a position, or None."""
        best = None
        for s, e, n in type_spans:
            if s < pos < e and (best is None or s > best[0]):
                best = (s, n)
        return best[1] if best else None

    for m in pattern.finditer(text):
        # Walk to the matching close brace.
        i = m.end()
        depth = 1
        while i < len(text) and depth:
            if text[i] == "{":
                depth += 1
            elif text[i] == "}":
                depth -= 1
            i += 1
        body = text[m.end(): i - 1]
        # Strip /* */ blocks before parsing members. Several enums keep
        # commented-out members that way (TreasureClass has a 17-name block),
        # and counting them as real shifts every auto-incremented value after
        # the block - silently, and by exactly the number of names inside it.
        body = re.sub(r"/\*.*?\*/", "", body, flags=re.S)
        # Members are comma-separated, not line-separated: C# permits
        # `Strength = 0, Intelligence, Willpower, ...` on one line, and a
        # line-based split reads that as a single member whose initialiser is
        # `0, Intelligence, ...`. Split on commas at paren depth 0 instead,
        # carrying each member's trailing comment with it.
        members = []
        buf, depth, comment = "", 0, ""
        def flush(buf, comment):
            s = buf.strip()
            if not s or s.startswith("["):
                return
            if "=" in s:
                nm, expr = s.split("=", 1)
                members.append((nm.strip(), expr.strip(), comment.strip()))
            else:
                members.append((s, None, comment.strip()))
        i2 = 0
        while i2 < len(body):
            ch = body[i2]
            # strip comments, keeping the text for the member being built
            if ch == "/" and i2 + 1 < len(body) and body[i2 + 1] == "/":
                end = body.find("\n", i2)
                end = len(body) if end < 0 else end
                c = body[i2 + 2:end].strip()
                # a comment before any member text belongs to the next member
                comment = c if buf.strip() else comment
                i2 = end
                continue
            if ch in "([":
                depth += 1
            elif ch in ")]":
                depth -= 1
            if ch == "," and depth == 0:
                flush(buf, comment)
                buf, comment = "", ""
            else:
                buf += ch
            i2 += 1
        flush(buf, comment)
        owner = enclosing(m.start())
        qualified = f"{owner}{m.group('name')}" if owner else m.group("name")
        yield (
            qualified,
            CS_TO_RUST.get(m.group("base") or "int", "i32"),
            "[Flags]" in ((m.group("attrs") or "") + (m.group("inline") or "")),
            (m.group("doc") or "").strip(),
            members,
        )


# A plain integer literal, which is what almost every member is. Matching this
# first matters: the identifier-substitution path below is O(members) per member,
# and these files have 31k members, so taking it for every one is ~1e9 regex
# operations. Fast-pathing literals turns that into a linear scan.
_LITERAL = re.compile(r"^-?(?:0[xX][0-9a-fA-F]+|\d+)[uUlL]*$")


def _eval_member(expr, env):
    """One member's initialiser -> int, or None if it cannot be resolved yet."""
    # Collapse whitespace first. C# wraps long composites across lines:
    #     Item = MeleeWeapon | Armor |
    #            Gem | SpellComponents,
    # and Python's eval rejects a bare newline inside an expression, so those
    # were failing for a reason that had nothing to do with the identifiers.
    e = " ".join(expr.split())
    if _LITERAL.match(e):
        return int(re.sub(r"[uUlL]+$", "", e), 0)
    cm = re.fullmatch(r"'(\\?.)'", e)
    if cm:
        s = cm.group(1)
        if s.startswith("\\"):
            return {"0": 0, "n": 10, "r": 13, "t": 9}.get(s[-1], ord(s[-1]))
        return ord(s[-1])
    e = re.sub(r"\((?:byte|sbyte|short|ushort|int|uint|long|ulong)\)", "", e)
    # Strip C# numeric suffixes, anchored on a digit-led literal.
    #
    # This was `(?<=[0-9a-fA-F])[uUlL]+\b`, which silently truncated any
    # identifier ending in a hex-digit letter followed by l/L/u/U:
    # `ResistItemAppraisal` -> `ResistItemAppraisa`. Requiring the token to
    # start with a digit makes identifiers unreachable.
    e = re.sub(r"\b(0[xX][0-9a-fA-F]+|\d+)[uUlL]+\b", r"\1", e)
    e = re.sub(r"\b\w+\.(\w+)\b", r"\1", e)  # Other.Member -> Member
    for tok in set(re.findall(r"\b[A-Za-z_]\w*\b", e)):
        if tok in env:
            e = re.sub(rf"\b{re.escape(tok)}\b", str(env[tok]), e)
    try:
        return eval(e, {"__builtins__": {}}, {})
    except Exception:
        return None


def resolve(members, base, shared=None):
    """Resolve C# discriminant expressions to integers, C#'s auto-increment rules.

    Two-phase, because **C# allows forward references within an enum**:

        PortalMagicTarget = Portal | LifeStone,   // Portal declared *below*

    A single left-to-right pass drops those. Phase 1 assigns literals and
    auto-increments (which are strictly positional and cannot forward-reference);
    phase 2 iterates the remaining expressions to a fixpoint.

    `shared` carries members of every enum already parsed in this file, so a
    composite referencing another enum (`CoverageMask.A | CoverageMask.B`)
    resolves too.
    """
    env = dict(shared or {})
    slots, nxt = [], 0
    # Phase 1: positional values.
    for name, expr, comment in members:
        if expr is None:
            val = nxt
        else:
            val = _eval_member(expr, env)
        if val is not None:
            env[name] = val
            nxt = val + 1
        slots.append([name, val, comment, expr])

    # Phase 2: fixpoint over whatever is still unresolved.
    for _ in range(len(slots) + 1):
        progressed = False
        for s in slots:
            if s[1] is None and s[3] is not None:
                v = _eval_member(s[3], env)
                if v is not None:
                    s[1] = v
                    env[s[0]] = v
                    progressed = True
        if not progressed:
            break

    if shared is not None:
        shared.update({s[0]: s[1] for s in slots if s[1] is not None})
    return [(s[0], s[1], s[2]) for s in slots]


def doc_lines(doc: str, indent: str = "") -> str:
    if not doc:
        return ""
    out = []
    for l in doc.split("\n"):
        l = l.strip()
        if l.startswith("///"):
            l = l[3:].strip()
        l = re.sub(r"</?summary>", "", l).strip()
        if l:
            out.append(f"{indent}/// {l}")
    return "\n".join(out) + ("\n" if out else "")


def emit(name, base, is_flags, doc, resolved) -> str:
    numeric = [(n, v, c) for n, v, c in resolved if v is not None]
    skipped = [(n, c) for n, v, c in resolved if v is None]
    lines = []

    if is_flags:
        lines.append("bitflags::bitflags! {")
        lines.append(doc_lines(f"/// C# `[Flags] enum {name} : {base}`.", "    ").rstrip("\n")
                     or f"    /// C# `[Flags] enum {name} : {base}`.")
        if doc:
            lines.append(doc_lines(doc, "    ").rstrip("\n"))
        lines.append("    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]")
        lines.append(f"    pub struct {name}: {base} {{")
        for n, v, c in numeric:
            tail = f" // {c}" if c else ""
            lines.append(f"        const {n} = {v:#x};{tail}")
        lines.append("    }")
        lines.append("}")
        if skipped:
            # This branch used to return without reporting, so an unresolved
            # member in a [Flags] enum vanished silently.
            lines.append("")
            lines.append(f"// NOT GENERATED from {name}: {len(skipped)} member(s) whose C#")
            lines.append("// initialiser could not be resolved. Listed so the gap is visible:")
            for n, c in skipped[:20]:
                lines.append(f"//   {n}" + (f"  // {c}" if c else ""))
        return "\n".join(lines) + "\n"

    # Plain enum. C# permits duplicate discriminants; Rust does not, so the
    # first member wins the variant and the rest become associated consts.
    seen, variants, aliases = {}, [], []
    for n, v, c in numeric:
        if v in seen:
            aliases.append((n, seen[v], v, c))
        else:
            seen[v] = n
            variants.append((n, v, c))

    if doc:
        lines.append(doc_lines(doc).rstrip("\n"))
    lines.append(f"/// C# `enum {name} : {base}`.")
    if aliases:
        lines.append(f"///")
        lines.append(f"/// {len(aliases)} C# member(s) share a discriminant with an earlier one, which")
        lines.append(f"/// Rust enums disallow; those are associated consts below.")
    lines.append(f"#[repr({base})]")
    lines.append("#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]")
    lines.append(f"pub enum {name} {{")
    for n, v, c in variants:
        tail = f" // {c}" if c else ""
        lines.append(f"    {ident(n)} = {v:#x},{tail}")
    lines.append("}")
    lines.append("")
    lines.append(f"impl {name} {{")
    for n, first, v, c in aliases:
        tail = f" // {c}" if c else ""
        lines.append(f"    /// Alias of [`Self::{ident(first)}`].")
        lines.append(f"    pub const {ident(n)}: Self = Self::{ident(first)};{tail}")
    lines.append(f"    /// Decode from the on-disk value. `None` for anything undefined —")
    lines.append(f"    /// a C-style cast would produce a value outside the enum, which is UB.")
    lines.append(f"    pub fn from_raw(v: {base}) -> Option<Self> {{")
    lines.append("        Some(match v {")
    for n, v, _ in variants:
        lines.append(f"            {v:#x} => Self::{ident(n)},")
    lines.append("            _ => return None,")
    lines.append("        })")
    lines.append("    }")
    lines.append(f"    /// The on-disk value.")
    lines.append(f"    pub fn to_raw(self) -> {base} {{")
    lines.append(f"        self as {base}")
    lines.append("    }")
    lines.append("}")
    if skipped:
        lines.append("")
        lines.append(f"// NOT GENERATED from {name}: {len(skipped)} member(s) whose C# initialiser")
        lines.append("// could not be resolved to a constant. Listed so the gap is visible:")
        for n, c in skipped[:20]:
            lines.append(f"//   {n}" + (f"  // {c}" if c else ""))
    return "\n".join(lines) + "\n"


def main() -> int:
    if len(sys.argv) != 4:
        print(__doc__)
        return 2
    src, dst, port_source = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
    text = src.read_text(encoding="utf-8-sig", errors="replace")
    enums = list(parse_enums(text))
    if not enums:
        print(f"no enums found in {src}")
        return 1

    # Two enums can still share a qualified name (the same name twice inside one
    # type, or at file scope). Suffix the later ones rather than emit a
    # collision, and record it so the mapping stays traceable.
    seen_names: dict[str, int] = {}
    fixed = []
    for name, base, is_flags, doc, members in enums:
        n = seen_names.get(name, 0)
        seen_names[name] = n + 1
        fixed.append((f"{name}{n + 1}" if n else name, base, is_flags, doc, members))
    enums = fixed

    body, stats = [], []
    any_flags = False
    shared: dict[str, int] = {}
    for name, base, is_flags, doc, members in enums:
        resolved = resolve(members, base, shared)
        body.append(emit(name, base, is_flags, doc, resolved))
        any_flags |= is_flags
        n_ok = sum(1 for _, v, _ in resolved if v is not None)
        stats.append((name, base, is_flags, len(members), n_ok))

    header = [
        f"// PORT-SOURCE: {port_source}",
        "// PORT-SHA: PLACEHOLDER",
        "// PORT-STATUS: done",
        "// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.",
        "//",
        f"// {len(enums)} enum(s), {sum(s[3] for s in stats)} members, generated from the C#",
        "// rather than transcribed. At this size a one-digit typo would mis-identify a",
        "// game asset in a way no test would obviously catch.",
        "//",
        "// C# allows duplicate discriminants within an enum; Rust does not. Where that",
        "// happens the first member becomes the variant and the rest become associated",
        "// consts pointing at it, so every C# name still resolves.",
        "//",
        "// Per enum:",
    ]
    for name, base, is_flags, total, ok in stats:
        kind = "bitflags" if is_flags else "enum"
        note = "" if ok == total else f"  ({total - ok} unresolved)"
        header.append(f"//   {name:<34} {base:<5} {kind:<8} {ok:>6} members{note}")
    header += ["", "#![allow(non_camel_case_types, non_upper_case_globals)]", ""]

    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_text("\n".join(header) + "\n" + "\n".join(body))
    total_m = sum(s[3] for s in stats)
    total_ok = sum(s[4] for s in stats)
    print(f"{src.name}: {len(enums)} enums, {total_ok}/{total_m} members -> {dst}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
