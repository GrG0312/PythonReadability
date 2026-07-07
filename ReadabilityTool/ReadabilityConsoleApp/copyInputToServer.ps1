param(
    [Parameter(Mandatory=$true, HelpMessage="The path of the local file to copy.")]
    [string]$FilePath
)

# Validate the local file exists
if (-not (Test-Path -Path $FilePath)) {
    Write-Error "Local file not found: $FilePath"
    exit 1
}

# Format the destination as alias:/path/to/destination
$destination = "nipg38:~/gergo_munka/input/"

Write-Host "Copying '$FilePath' to '$destination'..." -ForegroundColor Cyan

# Execute the scp command
scp $FilePath $destination

if ($LASTEXITCODE -eq 0) {
    Write-Host "File copied successfully!" -ForegroundColor Green
} else {
    Write-Error "Failed to copy the file. SCP command exited with code $LASTEXITCODE."
}