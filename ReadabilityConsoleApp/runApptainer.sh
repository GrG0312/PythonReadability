#!/bin/bash

# Configuration
GGUF_DIR="/ssd/bszalontai_local/models_gguf"                          
OUTPUT_DIR="/home/bszalontai/gergo_munka/output"          
DATASET_DIR="/home/bszalontai/gergo_munka/input"
SIF_PATH="/home/bszalontai/gergo_munka/app/readabilityconsoleapp.sif"

GPU_IDS=""
ARGS=()

while [[ $# -gt 0 ]]; do
    case $1 in
        --gpu)
            GPU_IDS="$2"
            shift 2
            ;;
        *)
            ARGS+=("$1")
            shift
            ;;
    esac
done

if [ -z "$GPU_IDS" ]; then
    echo "❌ No GPU specified. Use --gpu <id>"
    exit 1
fi

echo "Using GPU(s): $GPU_IDS"

# Check SIF file exists
if [ ! -f "$SIF_PATH" ]; then
    echo "❌ SIF file not found: $SIF_PATH"
    exit 1
fi

# Create output and dataset directories if they don't exist
mkdir -p "$OUTPUT_DIR"
mkdir -p "$DATASET_DIR"

# Run the Apptainer container
apptainer run \
    --nv \
    --env CUDA_VISIBLE_DEVICES="$GPU_IDS" \
    --bind "$GGUF_DIR:/models:ro" \
    --bind "$DATASET_DIR:/datasets:ro" \
    --bind "$OUTPUT_DIR:/output" \
    "$SIF_PATH" \
    "${ARGS[@]}" || echo "❌ Apptainer run failed with exit code $?"