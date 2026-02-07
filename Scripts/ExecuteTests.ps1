$projectPath = "../ImageSorter.sln"


# Build for Linux using WSL
Write-Host "Executing Tests on Linux using WSL..."
wsl dotnet test $projectPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed Tests on Linux."
    exit $LASTEXITCODE
}

# Build for Windows
Write-Host "Executing Tests on Windows..."
dotnet test $projectPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed Tests on Windows."
    exit $LASTEXITCODE
}

Write-Host "All Tests green"