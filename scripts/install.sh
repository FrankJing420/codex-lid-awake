#!/bin/zsh
set -euo pipefail

repo_dir="${0:A:h:h}"
source_app="$repo_dir/dist/Codex Lid Awake.app"
destination_app="/Applications/Codex Lid Awake.app"
daemon_plist="/Library/LaunchDaemons/local.codex.lidawake-reset.plist"

if [[ ! -d "$source_app" ]]; then
	print -u2 "Build the app first: ./scripts/build-app.sh"
	exit 1
fi

if [[ -e "$destination_app" ]]; then
	print -u2 "Refusing to overwrite $destination_app. Remove it first if you intend to replace it."
	exit 1
fi

sudo /usr/bin/ditto "$source_app" "$destination_app"
sudo /usr/bin/install -o root -g wheel -m 644 "$repo_dir/resources/local.codex.lidawake-reset.plist" "$daemon_plist"
sudo /bin/launchctl bootout system/local.codex.lidawake-reset >/dev/null 2>&1 || true
sudo /bin/launchctl bootstrap system "$daemon_plist"

print "Installed: $destination_app"
print "The reset daemon restores normal lid sleep after every reboot."
