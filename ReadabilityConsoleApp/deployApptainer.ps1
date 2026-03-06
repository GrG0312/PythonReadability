# This script deploys the ReposcraperConsole application to a remote server using Apptainer.

# Configuration variables
$SERVER = "nipg38"
$DEPLOY_PATH = "/home/bszalontai/gergo_munka/reposcraper"

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
# Excludes: deploy scripts, git, build artifacts, VS files
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
echo 'Building Apptainer image (this may take 10-15 minutes)...' && \
apptainer build --fakeroot readabilityconsoleapp.sif ReadabilityConsoleApp/readabilityconsoleapp.def && \
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
Write-Host "Direct run:"
Write-Host "  ./runApptainer.sh --gguf-path /models/<model>.gguf --repo-url <url> --language <lang> --extraction-type <type> --output-path /output/<file>.json"
Write-Host ""
Write-Host "Background run (recommended for long jobs):"
Write-Host "  ./runTmux.sh --gguf-path /models/<model>.gguf --repo-url <url> --language <lang> --extraction-type <type> --output-path /output/<file>.json"
Write-Host ""
Write-Host "Check running jobs:"
Write-Host "  ./checkTmux.sh"