#!/bin/bash

# Configuration
GGUF_DIR="/ssd/bszalontai_local/models_gguf"                          
OUTPUT_DIR="/home/bszalontai/gergo_munka/readabilityconsoleapp/output"          
DATASET_DIR="/home/bszalontai/gergo_munka/readabilityconsoleapp/datasets"
SIF_PATH="/home/bszalontai/gergo_munka/readabilityconsoleapp/readabilityconsoleapp.sif"   
DEFAULT_GPU_IDS="3" 

GPU_IDS="$DEFAULT_GPU_IDS"
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

echo "Using GPU(s): $GPU_IDS"

# Create output and dataset directories if they don't exist
mkdir -p "$OUTPUT_DIR"
mkdir -p "$DATASET_DIR"

# Run the Apptainer container
# --nv: Enable NVIDIA GPU support
# --bind: Mount directories
apptainer run \
    --nv \
    --env CUDA_VISIBLE_DEVICES="$GPU_IDS" \
    --bind "$GGUF_DIR:/models:ro" \
    --bind "$DATASET_DIR:/datasets:ro" \
    --bind "$OUTPUT_DIR:/output" \
    "$SIF_PATH" \
    "${ARGS[@]}"