#!/usr/bin/env python3
"""Delimiter-balance check over ported .rs files.

Single-pass tokenizer rather than regex passes. The earlier version stripped
`//` comments first and string literals second, so a `//` *inside* a string —
`"{}://{host}"`, or any URL — was treated as a comment start and swallowed the
rest of the line, including its closing delimiters. That misreported every file
containing a URL literal.

Handles: line comments, nested block comments (Rust allows nesting), normal and
raw strings (`r"..."`, `r#"..."#`), char literals, and byte strings.
"""
import sys
from pathlib import Path

PAIRS = {"{": "}", "(": ")", "[": "]"}
CLOSERS = {v: k for k, v in PAIRS.items()}


def strip(src: str) -> str:
    out, i, n = [], 0, len(src)
    while i < n:
        c = src[i]
        # line comment
        if c == "/" and i + 1 < n and src[i + 1] == "/":
            j = src.find("\n", i)
            i = n if j < 0 else j
            continue
        # block comment, nestable
        if c == "/" and i + 1 < n and src[i + 1] == "*":
            depth, i = 1, i + 2
            while i < n and depth:
                if src.startswith("/*", i):
                    depth += 1
                    i += 2
                elif src.startswith("*/", i):
                    depth -= 1
                    i += 2
                else:
                    i += 1
            continue
        # raw string: r"..." or r#*"..."#*
        if c == "r" and i + 1 < n and src[i + 1] in '#"':
            j = i + 1
            hashes = 0
            while j < n and src[j] == "#":
                hashes += 1
                j += 1
            if j < n and src[j] == '"':
                term = '"' + "#" * hashes
                k = src.find(term, j + 1)
                i = n if k < 0 else k + len(term)
                continue
        # byte string / byte char
        if c == "b" and i + 1 < n and src[i + 1] in "\"'":
            c, i = src[i + 1], i + 1
        # normal string
        if c == '"':
            i += 1
            while i < n:
                if src[i] == "\\":
                    i += 2
                    continue
                if src[i] == '"':
                    i += 1
                    break
                i += 1
            continue
        # char literal (or a lifetime, which has no closing quote)
        if c == "'":
            if src.startswith("\\", i + 1):
                j = src.find("'", i + 2)
                i = n if j < 0 else j + 1
            elif i + 2 < n and src[i + 2] == "'":
                i += 3
            else:
                i += 1  # lifetime
            continue
        out.append(c)
        i += 1
    return "".join(out)


def main() -> int:
    bad = 0
    files = [
        p for p in Path(".").rglob("*.rs")
        if "PORT-STATUS: done" in p.read_text(errors="replace")
    ]
    for p in files:
        s = strip(p.read_text(errors="replace"))
        stack, err = [], None
        for ch in s:
            if ch in PAIRS:
                stack.append(ch)
            elif ch in CLOSERS:
                if not stack:
                    err = f"unmatched {ch!r}"
                    break
                want = stack.pop()
                if PAIRS[want] != ch:
                    err = f"{want!r} closed by {ch!r}"
                    break
        if err is None and stack:
            err = f"unclosed {stack[-1]!r} ({len(stack)} open)"
        if err:
            print(f"UNBALANCED {p}: {err}")
            bad += 1
    print(f"checked {len(files)} ported files; {bad} imbalanced")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
