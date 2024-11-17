$projectPath = "../ImageSorter.sln"  # Path to your .NET 9 project
$outputPath = "../out"                 # Path to the output directory

# Create output directory if it doesn't exist
if (-not (Test-Path -Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath
}
else
{
    Remove-Item -Path "$outputPath/*" -Recurse -Force
}

# Build for Linux using WSL
Write-Host "Building for Linux using WSL..."
wsl dotnet publish $projectPath -c Release -r linux-x64

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build for Linux."
    exit $LASTEXITCODE
}
else
{
    Copy-Item "../ImageSorter/bin/Release/net9.0/linux-x64/publish/ImageSorter" "../out"
}

# Build for Windows
Write-Host "Building for Windows..."
dotnet publish $projectPath -c Release -r win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build for Windows."
    exit $LASTEXITCODE
}
else
{
    Copy-Item "../ImageSorter/bin/Release/net9.0/win-x64/publish/*.exe" "../out"
}


Write-Host "Build completed successfully."