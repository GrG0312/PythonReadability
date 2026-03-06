#!/bin/bash

echo ""
echo "========================================"
echo "📋 Active tmux Sessions"
echo "========================================"

# Check if any sessions exist
if tmux ls 2>/dev/null; then
    echo ""
else
    echo "No active sessions (all jobs completed)"
    echo ""
fi

echo "========================================"
echo "📁 Recent Log Files"
echo "========================================"
LOG_DIR="/home/bszalontai/gergo_munka/reposcraper/logs"

if [ -d "$LOG_DIR" ]; then
    ls -lt "$LOG_DIR" | head -6
else
    echo "No log directory found"
fi

echo ""
echo "========================================"
echo "🛠️  Useful Commands"
echo "========================================"
echo "  Monitor session:   tmux attach -t <session_name>"
echo "  View log:          tail -f $LOG_DIR/<log_file>"
echo "  Kill session:      tmux kill-session -t <session_name>"
echo "  Kill all sessions: tmux kill-server"
echo "========================================"
echo ""