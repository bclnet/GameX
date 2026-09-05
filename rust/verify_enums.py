#!/usr/bin/env python3
"""Independently re-parse the C# enums and compare every (enum, member) -> value
pair against the generated Rust. Deliberately does NOT share code with
gen_enums.py, so a bug in the generator's parsing cannot hide itself here."""
import re, sys
from pathlib import Path

def cs_pairs(path):
    cs = Path(path).read_text(encoding='utf-8-sig', errors='replace').replace('\r\n', '\n')
    # strip comments so they cannot be mistaken for members
    cs = re.sub(r'/\*.*?\*/', '', cs, flags=re.S)
    cs = re.sub(r'//[^\n]*', '', cs)
    # Same qualification the generator applies: a nested enum is named by its
    # enclosing type, because C# allows the same enum name in several classes.
    spans = []
    for tm in re.finditer(r'^[ \t]*(?:public|internal|private)?\s*'
                          r'(?:static\s+|sealed\s+|abstract\s+|partial\s+)*'
                          r'(?:class|struct|record)\s+(\w+)', cs, re.M):
        i, d, started = tm.end(), 0, False
        while i < len(cs):
            if cs[i] == '{': d += 1; started = True
            elif cs[i] == '}':
                d -= 1
                if started and d == 0: break
            i += 1
        spans.append((tm.start(), i, tm.group(1)))
    def owner(pos):
        best = None
        for s, e, n in spans:
            if s < pos < e and (best is None or s > best[0]): best = (s, n)
        return best[1] if best else None

    out, shared, seen = {}, {}, {}
    for m in re.finditer(r'public enum (\w+)(?:\s*:\s*\w+)?\s*\{', cs):
        i, depth = m.end(), 1
        while depth and i < len(cs):
            if cs[i] == '{': depth += 1
            elif cs[i] == '}': depth -= 1
            i += 1
        o = owner(m.start())
        base_name = f"{o}{m.group(1)}" if o else m.group(1)
        k = seen.get(base_name, 0); seen[base_name] = k + 1
        ename = f"{base_name}{k + 1}" if k else base_name
        body = cs[m.end():i-1]
        # split on top-level commas
        parts, buf, d = [], '', 0
        for ch in body:
            if ch in '([': d += 1
            elif ch in ')]': d -= 1
            if ch == ',' and d == 0: parts.append(buf); buf = ''
            else: buf += ch
        parts.append(buf)
        env, nxt, pend = dict(shared), 0, []
        for s in parts:
            s = s.strip()
            if not s or s.startswith('['): continue
            if '=' in s:
                n, e = s.split('=', 1); n, e = n.strip(), ' '.join(e.split())
                e = re.sub(r'\b(0[xX][0-9a-fA-F]+|\d+)[uUlL]+\b', r'\1', e)
                try: v = int(e, 0)
                except ValueError: pend.append((n, e)); continue
            else:
                n, v = s, nxt
            env[n] = v; nxt = v + 1
            out[(ename, n)] = v
        for _ in range(len(pend) + 1):
            again = []
            for n, e in pend:
                x = re.sub(r'\b\w+\.(\w+)\b', r'\1', e)
                for tok in set(re.findall(r'\b[A-Za-z_]\w*\b', x)):
                    if tok in env: x = re.sub(rf'\b{re.escape(tok)}\b', str(env[tok]), x)
                try:
                    v = eval(x, {'__builtins__': {}}, {})
                    env[n] = v; out[(ename, n)] = v
                except Exception:
                    again.append((n, e))
            if len(again) == len(pend): break
            pend = again
        shared.update(env)
    return out

def rs_pairs(path):
    rs = Path(path).read_text()
    out, cur, aliases = {}, None, {}
    for line in rs.split('\n'):
        em = re.match(r'pub enum (\w+)', line) or re.match(r'\s*pub struct (\w+):', line)
        if em: cur = em.group(1); continue
        # An alias (`pub const X: Self = Self::Y;`) is how a duplicate C#
        # discriminant is represented, since a Rust enum cannot repeat one.
        # Skipping those lines made every alias look like a missing member.
        am = re.match(r'\s+pub const (?:r#)?(\w+): Self = Self::(?:r#)?(\w+);', line)
        if am and cur:
            aliases.setdefault(cur, []).append((am.group(1), am.group(2)))
            continue
        vm = re.match(r'\s+(?:const )?(?:r#)?(\w+)(?::\s*Self)?\s*=\s*(-?0x[0-9a-fA-F]+|-?\d+)', line)
        if vm and cur: out[(cur, vm.group(1))] = int(vm.group(2), 0)
    return out

bad = 0
for cs, rs in [a.split(':') for a in sys.argv[1:]]:
    a, b = cs_pairs(cs), rs_pairs(rs)
    miss, extra = set(a) - set(b), set(b) - set(a)
    wrong = [(k, a[k], b[k]) for k in set(a) & set(b) if a[k] != b[k]]
    ok = not (miss or wrong)
    bad += 0 if ok else 1
    print(f"{'OK  ' if ok else 'FAIL'} {Path(cs).name:<22} C#={len(a):>6} Rust={len(b):>6} "
          f"missing={len(miss):>3} mismatched={len(wrong):>3} extra={len(extra):>3}")
    for k, x, y in wrong[:3]: print(f"       {k}: C#={x} Rust={y}")
    for k in list(miss)[:3]: print(f"       missing: {k} = {a[k]}")
sys.exit(1 if bad else 0)
