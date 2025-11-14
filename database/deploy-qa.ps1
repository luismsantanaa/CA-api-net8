# ========================================
# Deploy to QA Environment
# ========================================
# Description: Deploys SQL Server Database Project to QA/Testing environment
# Usage: .\deploy-qa.ps1 -Server "sql-qa.company.com" -Database "CleanArchitectureDb_QA"

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "sql-qa.company.com",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "CleanArchitectureDb_QA",
    
    [Parameter(Mandatory=$false)]
    [switch]$UseIntegratedSecurity = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$User = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = ""
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Deploy to QA Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server: $Server" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host "Auth: $(if($UseIntegratedSecurity){'Integrated Security'}else{'SQL Authentication'})" -ForegroundColor Yellow
Write-Host ""

# Confirm deployment
Write-Host "⚠️  You are about to deploy to QA environment" -ForegroundColor Yellow
$confirmation = Read-Host "Continue? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
    exit 0
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

# Prepare connection string
if ($UseIntegratedSecurity) {
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Encrypt=True;"
} else {
    if ([string]::IsNullOrEmpty($User) -or [string]::IsNullOrEmpty($Password)) {
        Write-Host "❌ User and Password are required when not using Integrated Security" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    $connectionString = "Server=$Server;Database=$Database;User Id=$User;Password=$Password;TrustServerCertificate=True;Encrypt=True;"
}

# Generate deployment script
Write-Host "`n📝 Generating deployment script for review..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$scriptPath = "..\deployment_scripts\QA_$timestamp.sql"

New-Item -ItemType Directory -Force -Path "..\deployment_scripts" | Out-Null

sqlpackage /Action:Script `
    /SourceFile:bin\Release\CleanArchitectureDb.dacpac `
    /TargetConnectionString:$connectionString `
    /OutputPath:$scriptPath `
    /p:BlockOnPossibleDataLoss=True

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to generate deployment script!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✅ Script generated: $scriptPath" -ForegroundColor Green

# Review script
Write-Host "`n📖 Please review the deployment script..." -ForegroundColor Yellow
$review = Read-Host "Open script in notepad for review? (yes/no)"
if ($review -eq "yes") {
    notepad $scriptPath
}

Write-Host "`nHave you reviewed and approved the deployment script?" -ForegroundColor Yellow
$scriptApproval = Read-Host "(yes/no)"
if ($scriptApproval -ne "yes") {
    Write-Host "❌ Deployment cancelled. Script saved for manual execution." -ForegroundColor Yellow
    Write-Host "   Script location: $scriptPath" -ForegroundColor Cyan
    Pop-Location
    exit 0
}

# Deploy
Write-Host "`n📤 Publishing to QA..." -ForegroundColor Yellow
$deployStart = Get-Date

sqlpackage /Action:Publish `
    /SourceFile:bin\Release\CleanArchitectureDb.dacpac `
    /TargetConnectionString:$connectionString `
    /p:BlockOnPossibleDataLoss=True `
    /p:IncludeCompositeObjects=True `
    /p:GenerateSmartDefaults=True `
    /p:BackupDatabaseBeforeChanges=True

$deployEnd = Get-Date
$duration = ($deployEnd - $deployStart).TotalSeconds

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Deployment to QA completed successfully!" -ForegroundColor Green
    Write-Host "⏰ Duration: $([math]::Round($duration, 2)) seconds" -ForegroundColor Cyan
    Write-Host "📊 Deployment script: $scriptPath" -ForegroundColor Cyan
    
    Write-Host "`n📋 Post-Deployment Checklist:" -ForegroundColor Yellow
    Write-Host "  ☐ Verify application connectivity" -ForegroundColor White
    Write-Host "  ☐ Run smoke tests" -ForegroundColor White
    Write-Host "  ☐ Verify seed data" -ForegroundColor White
    Write-Host "  ☐ Check application logs" -ForegroundColor White
    Write-Host "  ☐ Notify QA team" -ForegroundColor White
} else {
    Write-Host "`n❌ Deployment failed!" -ForegroundColor Red
    Write-Host "   Check the error messages above for details" -ForegroundColor Yellow
    Write-Host "   Deployment script saved at: $scriptPath" -ForegroundColor Cyan
    exit 1
}

Write-Host "`n========================================`n" -ForegroundColor Cyan

