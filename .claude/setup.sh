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
    echo "This script should be inside the repo at .claude/setup.sh"
    exit 1
fi

echo "Repo found at: $REPO_DIR"

# ─── 2. Restore global settings ───
CLAUDE_DIR="$HOME/.claude"
mkdir -p "$CLAUDE_DIR"

if [ ! -f "$SCRIPT_DIR/global-settings.json" ]; then
    echo "WARNING: .claude/global-settings.json is missing — skipping global settings."
    echo "You will need to set auth env vars by hand. See docs/HANDOFF.md."
elif [ -f "$CLAUDE_DIR/settings.json" ]; then
    echo ""
    echo "WARNING: ~/.claude/settings.json already exists."
    echo "Your existing file will NOT be overwritten."
    echo "Review .claude/global-settings.json and merge manually if needed."
else
    cp "$SCRIPT_DIR/global-settings.json" "$CLAUDE_DIR/settings.json"
    echo "Installed global settings to ~/.claude/settings.json"
    echo "  NOTE: this pins Vertex AI + project dev-tools-496118."
    echo "  Changing Claude accounts? See docs/HANDOFF.md before starting Claude."
fi

# ─── 3. Memory ───
# Memory lives IN THE REPO at .claude/memory and is tracked in git.
# .claude/settings.local.json sets "autoMemoryDirectory": ".claude/memory",
# so nothing needs copying — it travels with the clone. That is deliberate:
# the per-machine location (~/.claude/projects/<sanitized-path>/memory) does not
# survive a machine move or a re-clone to a different path.
if [ -d "$SCRIPT_DIR/memory" ]; then
    echo "Memory: in-repo at .claude/memory ($(ls -1 "$SCRIPT_DIR/memory"/*.md 2>/dev/null | wc -l) files) — nothing to install"
else
    echo "WARNING: .claude/memory is missing from the clone."
fi

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
echo "This project has been running Claude through Google Vertex AI."
echo ""
echo "OPTION A - staying on Vertex (same Google account):"
echo "  gcloud auth login"
echo "  gcloud auth application-default login"
echo "  gcloud config set project dev-tools-496118"
echo "  (gcloud not installed? https://cloud.google.com/sdk/docs/install)"
echo ""
echo "OPTION B - moving to a direct Anthropic / claude.ai account:"
echo "  Remove the Vertex block from ~/.claude/settings.json:"
echo "    CLAUDE_CODE_USE_VERTEX, ANTHROPIC_VERTEX_PROJECT_ID, CLOUD_ML_REGION"
echo "  Also drop the model pin if that model id is not on the new plan."
echo "  Then start Claude Code and run /login."
echo "  Leaving the Vertex env vars set will make Claude fail to start."
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
echo "  - Install Unity Hub + Editor 6000.5.8f1, add unity/neo-kyoto"
echo "  - Restart Claude Code to pick up MCP config"
echo ""
echo "NOT in this repo (gitignored on purpose — licence + LFS quota):"
echo "  - Purchased Unity kits. Re-import from the Unity Asset Store account:"
echo "      Cyberpunk Megapolis, Rolling Balls Sci-fi Pack, Cyber Box"
echo "    After importing Megapolis you MUST also run the URP unitypackage"
echo "    INSIDE the kit folder as a separate step, or everything renders pink."
echo "  - The AE/Grunge emission patch. Kit reimport reverts it."
echo "    See .claude/memory/project-vendor-shader-patch.md"
echo ""
echo "Read docs/HANDOFF.md first if changing machines or Claude accounts."
echo ""
echo "Start Claude Code in the repo:"
echo "  cd $REPO_DIR && claude"
