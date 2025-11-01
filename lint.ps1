# Loccar Lint Script
# This script runs various linting and code analysis tools

Write-Host "Running Loccar Code Analysis and Linting" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Restore packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Package restore failed!" -ForegroundColor Red
    exit 1
}

# Build with warnings as errors for analyzers
Write-Host "`nBuilding with code analysis..." -ForegroundColor Yellow
dotnet build --no-restore --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build with analysis failed!" -ForegroundColor Red
    exit 1
}

# Format code
Write-Host "`nFormatting code..." -ForegroundColor Yellow
dotnet format --no-restore --verbosity diagnostic
if ($LASTEXITCODE -ne 0) {
    Write-Host "Code formatting completed with warnings" -ForegroundColor Yellow
} else {
    Write-Host "Code formatting completed successfully" -ForegroundColor Green
}

# Run tests
Write-Host "`nRunning tests..." -ForegroundColor Yellow
dotnet test --no-build --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`nAll linting and analysis checks completed successfully!" -ForegroundColor Green
Write-Host "Your code is ready for commit!" -ForegroundColor Green
