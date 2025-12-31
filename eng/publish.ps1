# GitOut Native AOT Publishing Script (Windows)
# Builds self-contained Native AOT binaries for Windows platforms

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RepoRoot "src\GitOut.Console\GitOut.Console.csproj"
$ArtifactsDir = Join-Path $RepoRoot "artifacts\publish"

Write-Host "==> GitOut Native AOT Publishing Script" -ForegroundColor Cyan
Write-Host "==> Repository: $RepoRoot"
Write-Host "==> Artifacts: $ArtifactsDir"
Write-Host ""

# Target Runtime Identifiers for Windows
$RIDs = @(
    "win-x64",
    "win-arm64"
)

# Clean artifacts directory
Write-Host "==> Cleaning artifacts directory..." -ForegroundColor Yellow
if (Test-Path $ArtifactsDir) {
    Remove-Item -Path $ArtifactsDir -Recurse -Force
}
New-Item -Path $ArtifactsDir -ItemType Directory -Force | Out-Null

# Build for each RID
foreach ($rid in $RIDs) {
    Write-Host ""
    Write-Host "==> Publishing for $rid..." -ForegroundColor Cyan
    
    $OutputDir = Join-Path $ArtifactsDir $rid
    
    dotnet publish $ProjectPath `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishAot=true `
        -p:StripSymbols=true `
        -o $OutputDir
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Published $rid successfully" -ForegroundColor Green
        
        # Show output size
        $BinaryPath = Join-Path $OutputDir "gitout.exe"
        if (Test-Path $BinaryPath) {
            $Size = (Get-Item $BinaryPath).Length / 1MB
            Write-Host "  Binary size: $([math]::Round($Size, 2)) MB"
        }
    } else {
        Write-Host "✗ Failed to publish $rid" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "==> Publishing complete!" -ForegroundColor Green
Write-Host "==> Artifacts location: $ArtifactsDir"
Write-Host ""
Write-Host "Available binaries:"
Get-ChildItem -Path $ArtifactsDir -Recurse -Include "gitout.exe" | Select-Object -ExpandProperty FullName | Sort-Object
