#!/bin/bash
# Package the WebGL build for itch.io, and optionally push it with butler.
#
#   ./publish-webgl.sh            package only  -> dist/neo-kyoto-webgl.zip
#   ./publish-webgl.sh --push     package, then butler push to $ITCH_TARGET
#
# The zip has index.html at its root, which is what itch.io requires.
# Build first from the Unity menu: Neo-Kyoto > Build WebGL.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$SCRIPT_DIR/neo-kyoto/Builds/WebGL"
DIST_DIR="$SCRIPT_DIR/dist"
ZIP_PATH="$DIST_DIR/neo-kyoto-webgl.zip"
ITCH_TARGET="${ITCH_TARGET:-quinlayen/neo-kyoto:webgl}"

if [ ! -f "$BUILD_DIR/index.html" ]; then
    echo "ERROR: no WebGL build at $BUILD_DIR"
    echo "Build it first: Unity menu > Neo-Kyoto > Build WebGL"
    exit 1
fi

# Unity emits this folder and explicitly says not to ship it.
find "$BUILD_DIR" -maxdepth 1 -type d -name "*_DoNotShip*" -exec rm -rf {} + 2>/dev/null || true

mkdir -p "$DIST_DIR"
rm -f "$ZIP_PATH"

echo "Packaging $BUILD_DIR"
# Zip from inside the build dir so index.html sits at the zip root.
# Git Bash on Windows has no `zip`, so fall back to Python's zipfile.
if command -v zip >/dev/null 2>&1; then
    (cd "$BUILD_DIR" && zip -q -r -9 "$ZIP_PATH" .)
else
    python3 - "$BUILD_DIR" "$ZIP_PATH" <<'PYZIP'
import os, sys, zipfile
src, dst = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    for root, _, files in os.walk(src):
        for name in files:
            full = os.path.join(root, name)
            z.write(full, os.path.relpath(full, src))
PYZIP
fi

echo "Wrote $ZIP_PATH ($(du -h "$ZIP_PATH" | cut -f1))"
echo
echo "Zip root contents:"
python3 -c "import zipfile,sys; z=zipfile.ZipFile(sys.argv[1]); [print('  '+n) for n in z.namelist()[:5]]" "$ZIP_PATH"

if [ "$1" == "--push" ]; then
    if ! command -v butler >/dev/null 2>&1; then
        echo
        echo "ERROR: butler not found. Install it from:"
        echo "  https://itch.io/docs/butler/installing.html"
        echo "Then authenticate once with:  butler login"
        exit 1
    fi
    echo
    echo "Pushing to $ITCH_TARGET"
    butler push "$BUILD_DIR" "$ITCH_TARGET" --userversion-file "$SCRIPT_DIR/neo-kyoto/ProjectSettings/ProjectVersion.txt" 2>/dev/null \
        || butler push "$BUILD_DIR" "$ITCH_TARGET"
    echo "Done. Set the itch page to 'This file will be played in the browser'."
fi
