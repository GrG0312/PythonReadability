#!/bin/bash

# This script deploys the ReposcraperConsole application to a remote server using Apptainer.

# Configuration variables
SERVER="nipg38"
DEPLOY_PATH="/home/bszalontai/gergo_munka/reposcraper"

echo "========================================"
echo "Step 1: Building .NET application locally..."
echo "========================================"

dotnet publish ReadabilityConsoleApp/ReadabilityConsoleApp.csproj \
    -c Release \
    -o ./publish \
    /p:UseAppHost=false

if [ $? -ne 0 ]; then
    echo "❌ .NET build failed!"
    exit 1
fi

echo "✅ .NET build successful!"

echo ""
echo "========================================"
echo "Step 2: Creating deployment package..."
echo "========================================"

# Create tarball - only include what's needed on server
# Excludes: deploy scripts, git, build artifacts, VS files
tar -czf readabilityconsoleapp.tar.gz \
    --exclude='.git' \
    --exclude='**/bin' \
    --exclude='**/obj' \
    --exclude='*.user' \
    --exclude='.vs' \
    --exclude='*deploy*' \
    ./publish \
    ./ReadabilityConsoleApp/readabilityconsoleapp.def \
    ./ReadabilityConsoleApp/runApptainer.sh \
    ./ReadabilityConsoleApp/runTmux.sh \
    ./ReadabilityConsoleApp/checkTmux.sh

echo "✅ Deployment package created: readabilityconsoleapp.tar.gz"

echo ""
echo "========================================"
echo "Step 3: Transferring to server..."
echo "========================================"

scp readabilityconsoleapp.tar.gz "$SERVER:$DEPLOY_PATH/"

echo "✅ Transfer complete!"

echo ""
echo "========================================"
echo "Step 4: Building Apptainer image on server..."
echo "========================================"

ssh -t "$SERVER" "cd $DEPLOY_PATH && \
    tar -xzf readabilityconsoleapp.tar.gz && \
    echo 'Moving scripts to root directory...' && \
    mv ReadabilityConsoleApp/runApptainer.sh . && \
    mv ReadabilityConsoleApp/runTmux.sh . && \
    mv ReadabilityConsoleApp/checkTmux.sh . && \
    chmod +x runApptainer.sh runTmux.sh checkTmux.sh && \
    echo 'Building Apptainer image (this may take 10-15 minutes)...' && \
    apptainer build --fakeroot readabilityconsoleapp.sif ReadabilityConsoleApp/readabilityconsoleapp.def && \
    rm readabilityconsoleapp.tar.gz && \
    echo '✅ Apptainer image built successfully!'"

echo ""
echo "========================================"
echo "Step 5: Cleaning up local files..."
echo "========================================"

rm -rf ./publish
rm -f readabilityconsoleapp.tar.gz

echo "✅ Local cleanup complete!"

echo ""
echo "========================================"
echo "✅ Deployment complete!"
echo "========================================"
echo ""
echo "To run on server:"
echo "  ssh $SERVER"
echo "  cd $DEPLOY_PATH"
echo ""
echo "Direct run:"
echo "  ./runApptainer.sh --gguf-path /models/<model>.gguf --repo-url <url> --language <lang> --extraction-type <type> --output-path /output/<file>.json"
echo ""
echo "Background run (recommended for long jobs):"
echo "  ./runTmux.sh --gguf-path /models/<model>.gguf --repo-url <url> --language <lang> --extraction-type <type> --output-path /output/<file>.json"
echo ""
echo "Check running jobs:"
echo "  ./checkTmux.sh"