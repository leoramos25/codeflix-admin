#!/usr/bin/env python3
"""PreToolUse(Bash): starting the API must pin it to ports in 5000-5099 (see CLAUDE.md > Ports).

Covers `dotnet run`, `dotnet watch [options] [run]` and `dotnet <app>.dll`. The command is
tokenized with shlex so text inside quotes (commit messages, grep patterns) is not mistaken
for a command, and every URL of every `--urls`/`ASPNETCORE_URLS` is checked.
"""
import json
import re
import shlex
import sys

ALLOWED = range(5000, 5100)
SEPARATORS = {"&&", "||", ";", "|", "&", "\n", "(", ")"}
WRAPPERS = {"env", "nohup", "exec", "time", "command"}
NOT_RUN_UNDER_WATCH = {"test", "build", "publish", "msbuild"}


def deny(reason):
    print(json.dumps({
        "hookSpecificOutput": {
            "hookEventName": "PreToolUse",
            "permissionDecision": "deny",
            "permissionDecisionReason": reason,
        }
    }))
    sys.exit(0)


def strip_heredocs(text):
    """Drop heredoc bodies (file contents, not commands), keeping the line that opens them."""
    lines, out, delimiter = text.split("\n"), [], None
    for line in lines:
        if delimiter is not None:
            if line.strip() == delimiter:
                delimiter = None
            continue
        out.append(line)
        match = re.search(r"<<-?\s*(['\"]?)([A-Za-z_][A-Za-z0-9_]*)\1", line)
        if match:
            delimiter = match.group(2)
    return "\n".join(out)


def tokenize(text):
    lexer = shlex.shlex(text, posix=True, punctuation_chars="();<>|&\n")
    lexer.whitespace = " \t\r"
    lexer.whitespace_split = True
    return list(lexer)


def segments(tokens):
    current = []
    for token in tokens:
        if token in SEPARATORS:
            if current:
                yield current
            current = []
        else:
            current.append(token)
    if current:
        yield current


def starts_api(segment):
    """True when the segment launches the app; leading VAR=value and wrappers are skipped."""
    words = [w for w in segment if not re.match(r"^[A-Za-z_][A-Za-z0-9_]*=", w)]
    while words and words[0] in WRAPPERS:
        words = words[1:]
    if not words or words[0] != "dotnet":
        return False
    args = [w for w in words[1:] if not w.startswith("-")]
    if not args:
        return False
    if args[0] == "run" or args[0].endswith(".dll"):
        return True
    return args[0] == "watch" and not NOT_RUN_UNDER_WATCH & set(args[1:])


def urls_in(tokens):
    for i, token in enumerate(tokens):
        if token.startswith("ASPNETCORE_URLS="):
            yield token.split("=", 1)[1]
        elif token.startswith("--urls="):
            yield token.split("=", 1)[1]
        elif token == "--urls" and i + 1 < len(tokens):
            yield tokens[i + 1]


command = strip_heredocs(((json.load(sys.stdin).get("tool_input") or {}).get("command")) or "")
try:
    tokens = tokenize(command)
except ValueError:
    # Unbalanced quotes: fall back to a plain text check rather than letting it through.
    tokens = command.split()

if not any(starts_api(segment) for segment in segments(tokens)):
    sys.exit(0)

urls = [url for value in urls_in(tokens) for url in value.split(";") if url]
if not urls:
    deny(
        "Starting the API without an explicit port. "
        'Pass --urls "http://localhost:50XX" (or ASPNETCORE_URLS) with a port in 5000-5099.'
    )

without_port = [url for url in urls if not re.search(r":(\d+)/?$", url)]
if without_port:
    deny(f"URL(s) {without_port} have no explicit port. Use a port in 5000-5099.")

outside = [int(p) for url in urls for p in re.findall(r":(\d+)/?$", url) if int(p) not in ALLOWED]
if outside:
    deny(f"Port(s) {outside} outside the allowed 5000-5099 range. Pick the next free port inside it.")
