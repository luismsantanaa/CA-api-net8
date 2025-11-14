# ========================================
# Deploy to PRODUCTION Environment
# ========================================
# ⚠️ USE WITH EXTREME CAUTION ⚠️
#
# Description: Deploys SQL Server Database Project to PRODUCTION environment
# Usage: .\deploy-prod.ps1 -Server "sql-prod.company.com" -ChangeRequestNumber "CR-2025-1234"
#
# Requirements:
# - Approved Change Request
# - Recent backup verification
# - QA testing completed
# - Maintenance window scheduled

param(
    [Parameter(Mandatory=$true, HelpMessage="Production SQL Server name")]
    [string]$Server,
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "CleanArchitectureDb",
    
    [Parameter(Mandatory=$true, HelpMessage="Change Request Number (e.g., CR-2025-1234)")]
    [string]$ChangeRequestNumber,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseIntegratedSecurity = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$User = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Password = ""
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Red -BackgroundColor Yellow
Write-Host "  ⚠️  PRODUCTION DEPLOYMENT ⚠️" -ForegroundColor Red -BackgroundColor Yellow
Write-Host "========================================" -ForegroundColor Red -BackgroundColor Yellow
Write-Host ""
Write-Host "Server: $Server" -ForegroundColor Red
Write-Host "Database: $Database" -ForegroundColor Red
Write-Host "Change Request: $ChangeRequestNumber" -ForegroundColor Red
Write-Host "Deployed By: $env:USERNAME" -ForegroundColor Red
Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Red
Write-Host ""

# Multiple confirmation prompts
Write-Host "⚠️  This will modify the PRODUCTION database!" -ForegroundColor Red
Write-Host "   This action is irreversible and affects live users." -ForegroundColor Yellow
Write-Host ""

$confirmation1 = Read-Host "Type 'PRODUCTION' to continue (case-sensitive)"
if ($confirmation1 -cne "PRODUCTION") {
    Write-Host "❌ Deployment cancelled - incorrect confirmation" -ForegroundColor Yellow
    exit 0
}

$confirmation2 = Read-Host "Type the Change Request Number '$ChangeRequestNumber' to confirm"
if ($confirmation2 -ne $ChangeRequestNumber) {
    Write-Host "❌ Deployment cancelled - Change Request number mismatch" -ForegroundColor Red
    exit 0
}

# Pre-deployment checklist
Write-Host "`n📋 Pre-Deployment Checklist:" -ForegroundColor Yellow
Write-Host "   Please confirm the following items:" -ForegroundColor White
Write-Host ""

$checklist = @(
    "A full backup of PROD database was taken in the last hour",
    "Backup was verified and is restorable",
    "QA testing was completed successfully",
    "Change Request '$ChangeRequestNumber' is approved",
    "Maintenance window is scheduled and communicated",
    "Rollback plan is documented and ready",
    "Team is on standby for post-deployment verification"
)

foreach ($item in $checklist) {
    $response = Read-Host "   ☐ $item (yes/no)"
    if ($response -ne "yes") {
        Write-Host "`n❌ Pre-deployment checklist not completed" -ForegroundColor Red
        Write-Host "   Deployment cancelled for safety" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "`n✅ Pre-deployment checklist completed" -ForegroundColor Green

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
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=False;Encrypt=True;"
} else {
    if ([string]::IsNullOrEmpty($User) -or [string]::IsNullOrEmpty($Password)) {
        Write-Host "❌ User and Password are required when not using Integrated Security" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    $connectionString = "Server=$Server;Database=$Database;User Id=$User;Password=$Password;TrustServerCertificate=False;Encrypt=True;"
}

# Generate deployment script (MANDATORY for PROD)
Write-Host "`n📝 Generating deployment script (MANDATORY REVIEW)..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$scriptPath = "..\deployment_scripts\PROD_${ChangeRequestNumber}_$timestamp.sql"

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

# Mandatory script review
Write-Host "`n⚠️  MANDATORY: Review the deployment script before continuing!" -ForegroundColor Red
Write-Host "   This script will be executed against PRODUCTION" -ForegroundColor Yellow
Write-Host "   Press any key to open the script in notepad..." -ForegroundColor White
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

notepad $scriptPath

Write-Host "`nHave you thoroughly reviewed and approved the deployment script?" -ForegroundColor Yellow
$scriptReview = Read-Host "(yes/no)"
if ($scriptReview -ne "yes") {
    Write-Host "❌ Deployment cancelled - Script not approved" -ForegroundColor Yellow
    Write-Host "   Script saved for manual execution: $scriptPath" -ForegroundColor Cyan
    Pop-Location
    exit 0
}

# Final confirmation
Write-Host "`n⚠️  FINAL CONFIRMATION ⚠️" -ForegroundColor Red -BackgroundColor Yellow
Write-Host "   You are about to deploy to PRODUCTION" -ForegroundColor Red
Write-Host "   Server: $Server" -ForegroundColor Red
Write-Host "   Database: $Database" -ForegroundColor Red
Write-Host "   Change Request: $ChangeRequestNumber" -ForegroundColor Red
Write-Host ""
$finalConfirm = Read-Host "Type 'DEPLOY NOW' to proceed (case-sensitive)"
if ($finalConfirm -cne "DEPLOY NOW") {
    Write-Host "❌ Deployment cancelled - Final confirmation failed" -ForegroundColor Yellow
    Pop-Location
    exit 0
}

# Deploy
Write-Host "`n📤 Publishing to PRODUCTION..." -ForegroundColor Red
Write-Host "⏰ Started at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host ""

$deployStart = Get-Date

sqlpackage /Action:Publish `
    /SourceFile:bin\Release\CleanArchitectureDb.dacpac `
    /TargetConnectionString:$connectionString `
    /p:BlockOnPossibleDataLoss=True `
    /p:IncludeCompositeObjects=True `
    /p:GenerateSmartDefaults=True `
    /p:BackupDatabaseBeforeChanges=True `
    /p:DropObjectsNotInSource=False `
    /p:DoNotDropUsers=True `
    /p:DoNotDropLogins=True `
    /p:CommandTimeout=600

$deployEnd = Get-Date
$duration = $deployEnd - $deployStart

Pop-Location

# Log deployment details
$logPath = "..\deployment_scripts\PROD_deployment_log.txt"
$logEntry = @"

========================================
PRODUCTION DEPLOYMENT LOG
========================================
Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Change Request: $ChangeRequestNumber
Server: $Server
Database: $Database
Deployed By: $env:USERNAME
Computer: $env:COMPUTERNAME
Duration: $($duration.TotalMinutes) minutes
Status: $(if($LASTEXITCODE -eq 0){'SUCCESS'}else{'FAILED'})
Script: $scriptPath
========================================

"@

Add-Content -Path $logPath -Value $logEntry

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "✅ PRODUCTION DEPLOYMENT COMPLETED" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "⏰ Duration: $($duration.TotalMinutes) minutes" -ForegroundColor Cyan
    Write-Host "📊 Deployment script: $scriptPath" -ForegroundColor Cyan
    Write-Host "📋 Deployment log: $logPath" -ForegroundColor Cyan
    
    Write-Host "`n📋 Post-Deployment Checklist:" -ForegroundColor Yellow
    Write-Host "  ☐ Verify application connectivity" -ForegroundColor White
    Write-Host "  ☐ Run smoke tests on PROD" -ForegroundColor White
    Write-Host "  ☐ Monitor application logs for errors" -ForegroundColor White
    Write-Host "  ☐ Verify seed data executed correctly" -ForegroundColor White
    Write-Host "  ☐ Check health check endpoints" -ForegroundColor White
    Write-Host "  ☐ Monitor performance metrics" -ForegroundColor White
    Write-Host "  ☐ Update Change Request status to 'Deployed'" -ForegroundColor White
    Write-Host "  ☐ Notify stakeholders of successful deployment" -ForegroundColor White
    
    Write-Host "`n✅ Remember to monitor the application for the next hour!" -ForegroundColor Green
} else {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "❌ PRODUCTION DEPLOYMENT FAILED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "⚠️  Execute rollback plan immediately!" -ForegroundColor Red
    Write-Host "📊 Deployment script: $scriptPath" -ForegroundColor Cyan
    Write-Host "📋 Deployment log: $logPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🔄 Rollback Steps:" -ForegroundColor Yellow
    Write-Host "  1. Restore from backup taken before deployment" -ForegroundColor White
    Write-Host "  2. Verify restored database integrity" -ForegroundColor White
    Write-Host "  3. Test application connectivity" -ForegroundColor White
    Write-Host "  4. Notify stakeholders" -ForegroundColor White
    Write-Host "  5. Update Change Request with failure details" -ForegroundColor White
    exit 1
}

Write-Host "`n========================================`n" -ForegroundColor Green

