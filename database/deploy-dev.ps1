# ========================================
# Deploy to DEV Environment
# ========================================
# Description: Deploys SQL Server Database Project to local DEV environment (Docker)
# Usage: .\deploy-dev.ps1

param(
    [string]$Server = "localhost,11433",
    [string]$Database = "CleanArchitectureDb_DEV",
    [string]$User = "sa",
    [string]$Password = "YourPassword123!"
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Deploy to DEV Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server: $Server" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host ""

# Check if Docker SQL Server is running
Write-Host "🔍 Checking if SQL Server is running..." -ForegroundColor Yellow
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=$Server;User Id=$User;Password=$Password;TrustServerCertificate=True;Connect Timeout=5"
    $connection.Open()
    $connection.Close()
    Write-Host "✅ SQL Server is accessible" -ForegroundColor Green
} catch {
    Write-Host "❌ Cannot connect to SQL Server!" -ForegroundColor Red
    Write-Host "   Make sure Docker SQL Server is running:" -ForegroundColor Yellow
    Write-Host "   docker-compose up -d sqlserver" -ForegroundColor Cyan
    exit 1
}

# Build project
Write-Host "`n📦 Building SQL Project..." -ForegroundColor Yellow
Push-Location ".\CleanArchitectureDb"

$buildOutput = msbuild CleanArchitectureDb.sqlproj /p:Configuration=Release /v:minimal /nologo 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    Write-Host $buildOutput
    Pop-Location
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green

# Deploy
Write-Host "`n📤 Publishing to DEV..." -ForegroundColor Yellow
$connectionString = "Server=$Server;Database=$Database;User Id=$User;Password=$Password;TrustServerCertificate=True;Encrypt=True;"

$deployStart = Get-Date

sqlpackage /Action:Publish `
    /SourceFile:bin\Release\CleanArchitectureDb.dacpac `
    /TargetConnectionString:$connectionString `
    /p:BlockOnPossibleDataLoss=False `
    /p:IncludeCompositeObjects=True `
    /p:GenerateSmartDefaults=True

$deployEnd = Get-Date
$duration = ($deployEnd - $deployStart).TotalSeconds

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Deployment to DEV completed successfully!" -ForegroundColor Green
    Write-Host "⏰ Duration: $([math]::Round($duration, 2)) seconds" -ForegroundColor Cyan
    Write-Host "`n📋 Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Run the application: dotnet run --project src/AppApi" -ForegroundColor White
    Write-Host "  2. Access Swagger: https://localhost:7001/swagger" -ForegroundColor White
    Write-Host "  3. Run tests: dotnet test" -ForegroundColor White
} else {
    Write-Host "`n❌ Deployment failed!" -ForegroundColor Red
    Write-Host "   Check the error messages above for details" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n========================================`n" -ForegroundColor Cyan

