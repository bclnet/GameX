from __future__ import annotations
import zipfile

with zipfile.ZipFile('example.zip', 'r') as zf:
    # Read as bytes and decode to string
    content = zf.read('data.txt').decode('utf-8')
    print(content)