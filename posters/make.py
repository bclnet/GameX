import os, imgkit

options = {
    'enable-local-file-access': None,
    'width': 400
}
root = 'src'
for s in [s for s in os.listdir(root) if os.path.isdir(s)]:
    # if s != 'ID': continue
    for t in [t for t in os.listdir(os.path.join(root, s)) if t.endswith('.htm')]:
        imgkit.from_file(os.path.join(root, s, t), f'{s}/{t[:-4]}.png', options=options)