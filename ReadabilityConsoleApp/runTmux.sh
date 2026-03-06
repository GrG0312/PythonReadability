#!/bin/bash

# Configuration
LOG_DIR="/home/bszalontai/gergo_munka/readabilityconsoleapp/logs"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
SESSION_NAME="readabilityconsoleapp_$TIMESTAMP"
LOG_FILE="$LOG_DIR/run_$TIMESTAMP.log"

# Create log directory
mkdir -p "$LOG_DIR"

# Build the full command
CMD="cd /home/bszalontai/gergo_munka/readabilityconsoleapp && ./runApptainer.sh $* 2>&1 | tee $LOG_FILE; echo 'Job finished at $(date)' >> $LOG_FILE"

# Start detached tmux session
tmux new-session -d -s "$SESSION_NAME" "$CMD"

echo ""
echo "========================================"
echo "✅ Job started in background!"
echo "========================================"
echo "Session name: $SESSION_NAME"
echo "Log file:     $LOG_FILE"
echo ""
echo "Useful commands:"
echo "  Monitor live:    tmux attach -t $SESSION_NAME"
echo "  View log:        tail -f $LOG_FILE"
echo "  List sessions:   tmux ls"
echo "  Kill job:        tmux kill-session -t $SESSION_NAME"
echo "========================================"
echo ""
echo "Safely disconnecting from SSH is possible now!"
echo ""