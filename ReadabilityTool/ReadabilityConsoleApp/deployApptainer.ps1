# This script deploys the ReposcraperConsole application to a remote server using Apptainer.

# Configuration variables
$SERVER = "nipg38"
$DEPLOY_PATH = "/home/bszalontai/gergo_munka/app/"

Write-Host "========================================"
Write-Host "Step 1: Building .NET application locally..."
Write-Host "========================================"

dotnet publish ReadabilityConsoleApp/ReadabilityConsoleApp.csproj `
    -c Release `
    -o ./publish `
    /p:UseAppHost=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ .NET build successful!" -ForegroundColor Green

Write-Host ""
Write-Host "========================================"
Write-Host "Step 2: Creating deployment package..."
Write-Host "========================================"

# Create tarball - only include what's needed on server
tar -czf readabilityconsoleapp.tar.gz `
    --exclude='.git' `
    --exclude='**/bin' `
    --exclude='**/obj' `
    --exclude='*.user' `
    --exclude='.vs' `
    --exclude='*deploy*' `
    ./publish `
    ./ReadabilityConsoleApp/readabilityconsoleapp.def `
    ./ReadabilityConsoleApp/runApptainer.sh `
    ./ReadabilityConsoleApp/runTmux.sh `
    ./ReadabilityConsoleApp/checkTmux.sh

Write-Host "✅ Deployment package created: readabilityconsoleapp.tar.gz" -ForegroundColor Green

Write-Host ""
Write-Host "========================================"
Write-Host "Step 3: Transferring to server..."
Write-Host "========================================"

scp readabilityconsoleapp.tar.gz "${SERVER}:${DEPLOY_PATH}/"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Transfer failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Transfer complete!" -ForegroundColor Green

Write-Host ""
Write-Host "========================================"
Write-Host "Step 4: Building Apptainer image on server..."
Write-Host "========================================"

$remoteCommands = @"
cd $DEPLOY_PATH && \
tar -xzf readabilityconsoleapp.tar.gz && \
echo 'Moving scripts to root directory...' && \
mv ReadabilityConsoleApp/runApptainer.sh . && \
mv ReadabilityConsoleApp/runTmux.sh . && \
mv ReadabilityConsoleApp/checkTmux.sh . && \
chmod +x runApptainer.sh runTmux.sh checkTmux.sh && \
sed -i 's/\r//' runApptainer.sh && \
sed -i 's/\r//' runTmux.sh && \
sed -i 's/\r//' checkTmux.sh && \
echo 'Building Apptainer image (this may take 10-15 minutes)...' && \
cp ReadabilityConsoleApp/readabilityconsoleapp.def . && \
apptainer build --fakeroot readabilityconsoleapp.sif readabilityconsoleapp.def && \
rm readabilityconsoleapp.tar.gz && \
echo 'Apptainer image built successfully!'
"@

ssh -t $SERVER $remoteCommands

Write-Host ""
Write-Host "========================================"
Write-Host "Step 5: Cleaning up local files..."
Write-Host "========================================"

Remove-Item -Recurse -Force ./publish -ErrorAction SilentlyContinue
Remove-Item -Force readabilityconsoleapp.tar.gz -ErrorAction SilentlyContinue

Write-Host "✅ Local cleanup complete!" -ForegroundColor Green

Write-Host ""
Write-Host "========================================"
Write-Host "✅ Deployment complete!" -ForegroundColor Green
Write-Host "========================================"
Write-Host ""
Write-Host "To run on server:"
Write-Host "  ssh $SERVER"
Write-Host "  cd $DEPLOY_PATH"
Write-Host ""
Write-Host "Mode: metric direct run:"
Write-Host "  ./runApptainer.sh --mode metric --gguf-path /models/model.gguf --repo-url <url> --language <lang> --extraction-type <type> --output-path /output/out.json"
Write-Host ""
Write-Host "Mode: model background run (recommended):"
Write-Host "  ./runTmux.sh --mode model --dataset-path /datasets/my_dataset.parquet --gguf-dir /models --output-path /output/report.json"
Write-Host ""
Write-Host "Check running jobs:"
Write-Host "  ./checkTmux.sh"