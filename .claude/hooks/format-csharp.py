#!/usr/bin/env python3
"""PostToolUse(Write|Edit): format the edited .cs file with CSharpier (EF migrations excluded)."""
import json
import os
import subprocess
import sys

payload = json.load(sys.stdin)
path = (payload.get("tool_input") or {}).get("file_path") or ""

if not path.endswith(".cs") or "/Migrations/" in path or not os.path.isfile(path):
    sys.exit(0)

project_dir = os.environ.get("CLAUDE_PROJECT_DIR") or os.getcwd()
result = subprocess.run(
    ["dotnet", "csharpier", "format", path],
    cwd=project_dir,
    capture_output=True,
    text=True,
)
if result.returncode != 0:
    # Non-blocking: surface the formatter error to Claude without failing the edit.
    print(f"CSharpier failed on {path}:\n{result.stdout}{result.stderr}", file=sys.stderr)
    sys.exit(1)
