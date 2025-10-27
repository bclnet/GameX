import io, csv
from importlib import resources
from zstandard import ZstdDecompressor

with resources.files().joinpath('WAR.zst').open('rb') as f:
    file = io.StringIO(ZstdDecompressor().decompress(f.read()).decode('utf-8'))
    reader = csv.reader(file, delimiter='#')
    hashLookup: dict[int, str] = { int(f'0x{x[0]}{x[1]}', 16):x[2] for x in reader }
