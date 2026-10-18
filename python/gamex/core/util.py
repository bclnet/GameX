import os, random
from typing import Any

def _find(list, item):
    return list.index(item) if item in list else -1

def _valueF(elem: dict[str, Any], key: str, method: callable, default: Any = None) -> Any:
    return method(elem[key]) if key in elem else default
def _value(elem: dict[str, Any], key: str, default: Any = None) -> Any:
    return elem[key] if key in elem else default
def _valueBool(elem: dict[str, Any], key: str, default: bool = False) -> bool:
    if key not in elem: return default
    z = elem[key]
    if not isinstance(z, bool): raise ValueError(str(z))
    return z
def _valueV(elem: Any) -> Any:
    if isinstance(elem, bool): raise ValueError(str(elem))
    if isinstance(elem, int): return elem
    if isinstance(elem, float):
        if not elem.is_integer(): raise ValueError(str(elem))
        return int(elem)
    if isinstance(elem, str): return elem
    if isinstance(elem, list):
        for s in elem:
            if not isinstance(s, str): raise ValueError(str(s))
        return list(elem)
    if isinstance(elem, dict): return { k: _valueV(v) for k, v in elem.items() }
    raise ValueError(str(elem))

def _list(elem: dict[str, Any], key: str, func: Any = None, default: Any = None) -> Any:
    if func is not None and not callable(func): default = func; func = None
    if key not in elem: return default
    z = elem[key]
    if func is None: return z if isinstance(z, list) else [z]
    return _listV(z, func)
def _listV(elem: Any, func: Any = None) -> Any:
    def one(text: str): return func(text) if func else text
    if isinstance(elem, bool): raise ValueError(str(elem))
    if isinstance(elem, int): return [one(str(elem))]
    if isinstance(elem, float):
        if not elem.is_integer(): raise ValueError(str(elem))
        return [one(str(int(elem)))]
    if isinstance(elem, str): return [one(elem)]
    if isinstance(elem, list):
        out = []
        for s in elem:
            if not isinstance(s, str): raise ValueError(str(s))
            out.append(one(s))
        return out
    raise ValueError(str(elem))

# def _method(elem: dict[str, Any], key: str, method: callable, default: Any = None) -> Any:
#     return method(elem[key]) if key in elem else default

def _related(elem: dict[str, Any], key: str, method: callable, valueFunc: callable = None, default: Any = None) -> Any:
    if key not in elem: return {} if default is None else default
    z = elem[key]
    if valueFunc is not None: return { method(x): valueFunc(x) for x in z }
    return { k: method(k, v) for k, v in z.items() }

def _dictTrim(source: dict[str, Any]) -> Any:
    return { k:v for k,v in source.items() if v }

_random = None
def _randomValue(low: int, high: int) -> int:
    global _random
    if _random is None: _random = random.Random()
    return _random.randint(low, high)

_guess1 = {
    0x000001D8: '.motlist', 0x00424956: '.vib', 0x00444957: '.wid', 0x00444F4C: '.lod', 0x00444252: '.rbd', 0x004C4452: '.rdl',
    0x00424650: '.pfb', 0x00464453: '.mmtr', 0x0046444D: '.mdf2', 0x004C4F46: '.fol', 0x004E4353: '.scn', 0x004F4C43: '.clo',
    0x00504D4C: '.lmp', 0x00535353: '.sss', 0x00534549: '.ies', 0x00530040: '.wel', 0x00584554: '.tex', 0x00525355: '.user',
    0x005A5352: '.wcc', 0x04034B50: '.zip', 0x4D534C43: '.clsm', 0x54414D2E: '.mat', 0x54464453: '.sdft', 0x44424453: '.sdbd',
    0x52554653: '.sfur', 0x464E4946: '.finf', 0x4D455241: '.arem', 0x21545353: '.sst', 0x204D4252: '.rbm', 0x4D534648: '.hfsm',
    0x59444F42: '.rdd', 0x20464544: '.def', 0x4252504E: '.nprb', 0x44484B42: '.bnk', 0x75B22630: '.mov', 0x4853454D: '.mesh',
    0x4B504B41: '.pck', 0x50534552: '.spmdl', 0x54564842: '.fsmv2', 0x4C4F4352: '.rcol', 0x5556532E: '.uvs', 0x4C494643: '.cfil',
    0x54504E47: '.gnpt', 0x54414D43: '.cmat', 0x44545254: '.trtd', 0x50494C43: '.clip', 0x564D4552: '.mov', 0x414D4941: '.aimapattr',
    0x504D4941: '.aimp', 0x72786665: '.efx', 0x736C6375: '.ucls', 0x54435846: '.fxct', 0x58455452: '.rtex', 0x4F464246: '.oft',
    0x4C4F434D: '.mcol', 0x46454443: '.cdef', 0x504F5350: '.psop', 0x454D414D: '.mame', 0x43414D4D: '.mameac', 0x544C5346: '.fslt',
    0x64637273: '.srcd', 0x68637273: '.asrc', 0x4F525541: '.auto', 0x7261666C: '.lfar', 0x52524554: '.terr', 0x736E636A: '.jcns',
    0x6C626C74: '.tmlbld', 0x54455343: '.cset', 0x726D6565: '.eemr', 0x434C4244: '.dblc', 0x384D5453: '.stmesh', 0x32736674: '.tmlfsm2',
    0x45555141: '.aque', 0x46554247: '.gbuf', 0x4F4C4347: '.gclo', 0x44525453: '.srtd', 0x544C4946: '.filt',
}
_guess2 = {
    0x00766544: '.dev', 0x6E616863: '.chain', 0x6E6C6B73: '.fbxskel', 0x47534D47: '.msg', 0x52495547: '.gui', 0x47464347: '.gcfg',
    0x72617675: '.uvar', 0x544E4649: '.ifnt', 0x20746F6D: '.mot', 0x70797466: '.mov', 0x6D61636D: '.mcam', 0x6572746D: '.mtre',
    0x6D73666D: '.mfsm', 0x74736C6D: '.motlist', 0x6B6E626D: '.motbank', 0x3273666D: '.motfsm2', 0x74736C63: '.mcamlist',
    0x70616D6A: '.jmap', 0x736E636A: '.jcns', 0x4E414554: '.tean', 0x61646B69: '.ikda', 0x736C6B69: '.ikls', 0x72746B69: '.iktr',
    0x326C6B69: '.ikl2', 0x72686366: '.fchr', 0x544C5346: '.fslt', 0x6B6E6263: '.cbnk', 0x30474154: '.havokcl', 0x52504347: '.gcpr',
    0x74646366: '.fcmndatals', 0x67646C6A: '.jointlodgroup', 0x444E5347: '.gsnd', 0x59545347: '.gsty', 0x3267656C: '.leg2',
}

def _guessTail(buf) -> str: return f'.{bytes(buf[:3]).decode("ascii", "replace")}'.lower()

def _guessExtension(buf, fast: bool = True):
    if len(buf) < 4: return ''
    n = int.from_bytes(buf[:4], 'little')
    if fast: return '.asf' if n == 0x75B22630 else _guessTail(buf)
    if n in _guess1: return _guess1[n]
    n2 = int.from_bytes(buf[4:8], 'little') if len(buf) >= 8 else 0
    return _guess2[n2] if n2 in _guess2 else _guessTail(buf)

def grammerSize(i: int) -> str:
    t, c=['','K','M','G','T'], 0
    while i > 1024: i /= 1024; c += 1
    return f'{round(i, 2)}{t[c]}B'