#!/usr/bin/env bash
set -euo pipefail

REPO="https://github.com/dotnet/macios-devtools"
TARGET_BRANCH="main"

echo "=== Arcade Dependency Flow Setup ==="
echo ""

# 1. Check darc
if ! command -v darc &>/dev/null && [ ! -f ~/.dotnet/tools/darc ]; then
  echo "Installing darc CLI..."
  ./eng/common/darc-init.sh
  export PATH="$HOME/.dotnet/tools:$PATH"
fi

# 2. Verify authentication
if ! darc get-channels &>/dev/null 2>&1; then
  echo "darc is not authenticated. Running 'darc authenticate'..."
  echo "You will need a BAR token from https://maestro.dot.net/"
  darc authenticate
fi

# 3. Set default channel
echo ""
echo "Setting default channel..."
darc add-default-channel \
  --branch "refs/heads/$TARGET_BRANCH" \
  --repo "$REPO" \
  --channel ".NET Eng - Latest"

# 4. Subscribe to arcade updates
echo ""
echo "Creating arcade subscription..."
darc add-subscription \
  --channel ".NET Eng - Latest" \
  --source-repo https://github.com/dotnet/arcade \
  --target-repo "$REPO" \
  --target-branch "$TARGET_BRANCH" \
  --update-frequency everyDay \
  --standard-automerge

# 5. Verify
echo ""
echo "=== Subscriptions ==="
darc get-subscriptions --target-repo "$REPO"
echo ""
echo "=== Default Channels ==="
darc get-default-channels --source-repo "$REPO"
echo ""
echo "Done! Dependency flow is configured."
