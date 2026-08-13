#!/bin/bash
# Neo-Kyoto Claude Code Portable Setup
# Run this on a new machine after cloning the repo.
#
# Prerequisites:
#   - Claude Code installed (npm install -g @anthropic-ai/claude-code)
#   - Git, Node.js, Python 3
#   - Google Cloud SDK (gcloud) for Vertex AI auth

set -e
echo "=== Neo-Kyoto Claude Code Setup ==="
echo ""

# ─── 1. Detect the repo path ───
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR="$(dirname "$SCRIPT_DIR")"

if [ ! -f "$REPO_DIR/main.py" ]; then
    echo "ERROR: Can't find the neo-kyoto repo."
    echo "This script should be inside the repo at portable/setup.sh"
    exit 1
fi

echo "Repo found at: $REPO_DIR"

# ─── 2. Restore global settings ───
CLAUDE_DIR="$HOME/.claude"
mkdir -p "$CLAUDE_DIR"

if [ -f "$CLAUDE_DIR/settings.json" ]; then
    echo ""
    echo "WARNING: ~/.claude/settings.json already exists."
    echo "Your existing file will NOT be overwritten."
    echo "Review portable/global-settings.json and merge manually if needed."
else
    cp "$SCRIPT_DIR/global-settings.json" "$CLAUDE_DIR/settings.json"
    echo "Installed global settings to ~/.claude/settings.json"
fi

# ─── 3. Restore memory files ───
# Claude Code stores memory by sanitized project path.
# We need to figure out the correct path for this machine.
SANITIZED_PATH=$(echo "$REPO_DIR" | sed 's|/|-|g; s|^-||')
MEMORY_DIR="$CLAUDE_DIR/projects/-${SANITIZED_PATH}/memory"
mkdir -p "$MEMORY_DIR"

cp "$SCRIPT_DIR/memory/"* "$MEMORY_DIR/" 2>/dev/null || true
echo "Installed memory files to $MEMORY_DIR"

# ─── 4. Install Unity MCP server ───
MCP_DIR="$HOME/.local/share/unity-mcp-server"
if [ -d "$MCP_DIR" ]; then
    echo "Unity MCP server already installed at $MCP_DIR"
else
    echo "Installing Unity MCP server..."
    git clone https://github.com/AnkleBreaker-Studio/unity-mcp-server.git "$MCP_DIR"
    cd "$MCP_DIR" && npm install
    echo "Unity MCP server installed."
fi

# Update .mcp.json with correct path for this machine
cat > "$REPO_DIR/.mcp.json" << MCPEOF
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": ["$MCP_DIR/src/index.js"],
      "env": {
        "UNITY_BRIDGE_PORT": "7890"
      }
    }
  }
}
MCPEOF
echo "Updated .mcp.json with local paths"

# ─── 5. Google Cloud / Vertex AI auth ───
echo ""
echo "=== Authentication Setup ==="
echo ""
echo "This project uses Google Vertex AI for Claude."
echo "You need to authenticate with the same Google account."
echo ""
echo "Run these commands:"
echo ""
echo "  gcloud auth login"
echo "  gcloud auth application-default login"
echo "  gcloud config set project dev-tools-496118"
echo ""
echo "If gcloud is not installed:"
echo "  https://cloud.google.com/sdk/docs/install"
echo ""

# ─── 6. Summary ───
echo "=== Setup Complete ==="
echo ""
echo "Installed:"
echo "  - Global settings:  ~/.claude/settings.json"
echo "  - Memory files:     $MEMORY_DIR"
echo "  - Unity MCP server: $MCP_DIR"
echo "  - MCP config:       $REPO_DIR/.mcp.json"
echo ""
echo "Still needed:"
echo "  - Run gcloud auth (see above)"
echo "  - Install Unity Hub + Editor (for Unity MCP to work)"
echo "  - Restart Claude Code to pick up MCP config"
echo ""
echo "Start Claude Code in the repo:"
echo "  cd $REPO_DIR && claude"
