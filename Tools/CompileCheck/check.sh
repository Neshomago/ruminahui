#!/usr/bin/env bash
# Offline sanity check for the Unity C# code — no Unity install needed.
#   1) compiles Assets/Scripts (runtime) against Unity API stubs
#   2) compiles Editor tools + EditMode tests
#   3) runs the pure-logic EditMode tests
# Stubs encode the Unity API as we believe it to be: a clean pass here does NOT prove Unity API names are right.
set -euo pipefail
cd "$(dirname "$0")"
DOTNET="${DOTNET:-$(command -v dotnet || echo /usr/local/share/dotnet/dotnet)}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
for p in Runtime EditorAndTests; do
  echo "== Compile: $p"
  "$DOTNET" build "$p.csproj" --no-incremental -v q -nologo 2>&1 | grep -E "error|Error\(s\)|Warning\(s\)" || true
done
echo "== Dialogue data in sync with docs/04-dialogue-script.md"
python3 ../DialogueImport/import_dialogue.py --check
echo "== Run EditMode logic tests"
"$DOTNET" run --project TestRunner.csproj -v q 2>&1 | grep -E "PASS|FAIL|passed" 
