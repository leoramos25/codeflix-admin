#!/usr/bin/env python3
"""PreToolUse(Bash): `dotnet run` must pin the API to a port in 5000-5099 (see CLAUDE.md > Ports)."""
import json
import re
import sys

command = ((json.load(sys.stdin).get("tool_input") or {}).get("command")) or ""

if not re.search(r"\bdotnet\s+(watch\s+)?run\b", command):
    sys.exit(0)

ports = [int(p) for p in re.findall(r"(?:--urls[=\s]+|ASPNETCORE_URLS=)\S*?:(\d+)", command)]


def deny(reason):
    print(json.dumps({
        "hookSpecificOutput": {
            "hookEventName": "PreToolUse",
            "permissionDecision": "deny",
            "permissionDecisionReason": reason,
        }
    }))
    sys.exit(0)


if not ports:
    deny(
        "dotnet run without an explicit port falls back to launchSettings (5208). "
        'Pass --urls "http://localhost:50XX" with a port in 5000-5099.'
    )
outside = [p for p in ports if not 5000 <= p <= 5099]
if outside:
    deny(f"Port(s) {outside} outside the allowed 5000-5099 range. Pick the next free port inside it.")
