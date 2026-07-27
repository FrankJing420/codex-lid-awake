#!/bin/zsh
set -euo pipefail

repo_dir="${0:A:h:h}"
output_app="$repo_dir/dist/Codex Lid Awake.app"

mkdir -p "$repo_dir/dist"
rm -rf "$output_app"
/usr/bin/osacompile -o "$output_app" "$repo_dir/CodexLidAwake.applescript"
/usr/bin/codesign --verify --deep --strict "$output_app"

print "Built: $output_app"
