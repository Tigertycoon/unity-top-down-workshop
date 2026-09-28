"""Check tracked Unity assets for broken metadata and scene references, without an editor license."""
from pathlib import Path
import json
import re
import subprocess

root = Path(__file__).resolve().parents[1]
tracked = subprocess.check_output(["git", "ls-files"], cwd=root, text=True).splitlines()
assert tracked, "No tracked publication files found"
paths = set(tracked)
guids = {}
for name in tracked:
    path = root / name
    assert path.stat().st_size < 10_000_000, f"Unexpected large source asset: {name}"
    assert not any(part in name.split('/') for part in ('Library', 'Temp', 'Logs', 'UserSettings', 'Gif', 'RPGPowerForge'))
    if name.startswith('Assets/') and not name.endswith('.meta'):
        assert name + '.meta' in paths, f"Missing metadata: {name}"
    if name.endswith('.meta'):
        assert (root / name[:-5]).exists(), f"Orphan metadata: {name}"
        match = re.search(r'^guid: ([0-9a-f]{32})$', path.read_text(encoding='utf-8-sig'), re.M)
        assert match, f"Missing GUID: {name}"
        guid = match.group(1)
        assert guid not in guids, f"Duplicate GUID: {name} / {guids.get(guid)}"
        guids[guid] = name

references = 0
for name in tracked:
    if name.endswith(('.unity', '.prefab', '.asset')):
        for guid in re.findall(r'guid: ([0-9a-f]{32})', (root / name).read_text(encoding='utf-8-sig')):
            if guid.startswith('0000000000000000'):  # Unity built-in resources
                continue
            assert guid in guids, f"Unresolved asset reference in {name}: {guid}"
            references += 1

manifest = json.loads((root / 'Packages/manifest.json').read_text(encoding='utf-8-sig'))
assert all(name.startswith('com.unity.') and not version.startswith(('file:', 'http'))
           for name, version in manifest['dependencies'].items()), 'Unexpected local/external package source'
for name in ('Workshop_Template', 'Workshop_Extended'):
    assert f'Assets/Workshop/Scenes/{name}.unity' in paths
print(json.dumps({'tracked_files': len(tracked), 'asset_guids': len(guids),
                  'resolved_references': references, 'result': 'passed'}, indent=2))
