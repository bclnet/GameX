from importlib import resources
from zstandard import ZstdDecompressor

with resources.files().joinpath('TOR.zst').open('rb') as f:
    decompressed_data = ZstdDecompressor().decompress(f.read())
hashEntries: dict[int, str] = { }
