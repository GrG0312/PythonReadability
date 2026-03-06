#!/bin/bash

# Configuration
GGUF_DIR="/ssd/bszalontai_local/models_gguf"                          # Where GGUF files are stored on server
OUTPUT_DIR="/home/bszalontai/gergo_munka/readabilityconsoleapp/output"          # Where to save results
SIF_PATH="/home/bszalontai/gergo_munka/readabilityconsoleapp/readabilityconsoleapp.sif"   # Path to Apptainer image
DEFAULT_GPU_IDS="3" # Default GPU IDs if not set via environment variable

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

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Run the Apptainer container
# --nv: Enable NVIDIA GPU support
# --bind: Mount directories (similar to Docker's -v)
# "$@": Pass all script arguments to container
apptainer run \
    --nv \
    --env CUDA_VISIBLE_DEVICES="$GPU_IDS" \
    --bind "$GGUF_DIR:/models:ro" \
    --bind "$OUTPUT_DIR:/output" \
    "$SIF_PATH" \
    "${ARGS[@]}"