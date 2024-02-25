from pathlib import Path
import sys
import shutil

KEEPITEMS = [
    "Assets",
    "XFsm.dll",
    "XFsm.deps.json",
    "XFsm.pdb",
    "XFsm.runtimeconfig.json",
    "XFsm.Native.dll",
    "XFsm.Native.pdb",
    "XFsm.Native.lib",
    "XFsm.Native.exp",
    "GTranslate.dll",
]

def cleanup(path: Path):
    for item in path.iterdir():
        if item.name not in KEEPITEMS:
            if item.is_dir():
                shutil.rmtree(item)
            else:
                item.unlink()
    return

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: cleanup.py <path>")
        sys.exit(1)
    path = Path(sys.argv[1])
    cleanup(path)
    sys.exit(0)
