[CmdletBinding()]
param([switch]$IncludeSecurity)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$npmExecutable = if ($env:OS -eq 'Windows_NT') { 'npm.cmd' } else { 'npm' }
function Invoke-Checked([string]$Executable, [string[]]$Arguments) {
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Check failed: $Executable $($Arguments -join ' ') (exit $LASTEXITCODE)" }
}
Push-Location $repoRoot
try {
    foreach ($toolName in @('dotnet', $npmExecutable, 'docker', 'terraform', 'git')) {
        if (-not (Get-Command $toolName -ErrorAction SilentlyContinue)) { throw "Required tool missing: $toolName" }
    }
    Invoke-Checked dotnet @('restore', 'SecureIncidentHub.slnx', '--locked-mode')
    Invoke-Checked dotnet @('format', 'SecureIncidentHub.slnx', '--no-restore', '--verify-no-changes')
    Invoke-Checked dotnet @('build', 'SecureIncidentHub.slnx', '--no-restore', '-c', 'Release')
    Invoke-Checked dotnet @('test', 'SecureIncidentHub.slnx', '--no-build', '--no-restore', '-c', 'Release')
    Invoke-Checked $npmExecutable @('--prefix', 'src/frontend', 'ci', '--ignore-scripts')
    Invoke-Checked $npmExecutable @('--prefix', 'src/frontend', 'run', 'format:check')
    Invoke-Checked $npmExecutable @('--prefix', 'src/frontend', 'run', 'build')
    Invoke-Checked $npmExecutable @('--prefix', 'src/frontend', 'test')
    Invoke-Checked docker @('compose', '--env-file', 'infra/compose/.env.example', '-f', 'infra/compose/compose.yaml', 'config', '--quiet')
    Invoke-Checked terraform @('-chdir=infra/terraform/environments/lab', 'fmt', '-check')
    Invoke-Checked terraform @('-chdir=infra/terraform/environments/lab', 'init', '-backend=false', '-input=false', '-lockfile=readonly')
    Invoke-Checked terraform @('-chdir=infra/terraform/environments/lab', 'validate')
    & "$PSScriptRoot/test-foundation.ps1"
    if ($IncludeSecurity) {
        foreach ($toolName in @('gitleaks', 'trivy')) {
            if (-not (Get-Command $toolName -ErrorAction SilentlyContinue)) { throw "Security tool missing: $toolName" }
        }
        Invoke-Checked gitleaks @('git', '.', '--log-opts=--all', '--config', 'security/gitleaks.toml', '--redact')
        Invoke-Checked gitleaks @('git', '.', '--pre-commit', '--staged', '--config', 'security/gitleaks.toml', '--redact')
        Invoke-Checked $npmExecutable @('--prefix', 'src/frontend', 'audit', '--audit-level=high')
        Invoke-Checked trivy @('filesystem', '--config', 'security/trivy.yaml', '--scanners', 'vuln,misconfig', '.')
    }
} finally {
    Pop-Location
}
