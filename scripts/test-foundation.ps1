[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent

foreach ($file in (Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.ps1')) {
    $syntaxTokens = $null
    $syntaxErrors = $null
    [System.Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$syntaxTokens, [ref]$syntaxErrors) | Out-Null
    if ($syntaxErrors.Count -gt 0) { throw "PowerShell syntax failed: $($file.Name)" }
}

$global:SihPreflightTestCalls = [System.Collections.Generic.List[string]]::new()
$global:SihPreflightTestFailure = $false
function aws {
    $global:SihPreflightTestCalls.Add(($args -join ' '))
    if ($global:SihPreflightTestFailure) { $global:LASTEXITCODE = 1; return }
    $global:LASTEXITCODE = 0
    # Synthetic identity only. This function never invokes AWS CLI.
    '{"Account":"000000000000","Arn":"arn:aws:sts::000000000000:assumed-role/example/test"}'
}
function Assert-Fails([scriptblock]$Operation, [string]$ExpectedMessage) {
    try { & $Operation | Out-Null } catch {
        if ($_.Exception.Message -notlike $ExpectedMessage) { throw }
        return
    }
    throw 'Expected the operation to fail.'
}
$previousProfile = $env:AWS_PROFILE
$previousRegion = $env:AWS_REGION
try {
    $env:AWS_PROFILE = 'environment-profile'
    $env:AWS_REGION = 'us-east-1'
    $result = & "$PSScriptRoot/aws-preflight.ps1" -AwsProfile 'explicit-profile'
    if ($result.Profile -ne 'explicit-profile' -or $result.Account -ne '000000000000' -or $result.Region -ne 'us-east-1') {
        throw 'Explicit profile or identity readback failed.'
    }
    $result = & "$PSScriptRoot/aws-preflight.ps1"
    if ($result.Profile -ne 'environment-profile') { throw 'Environment profile fallback failed.' }
    $env:AWS_REGION = $null
    $result = & "$PSScriptRoot/aws-preflight.ps1"
    if ($result.Region -ne 'us-east-1') { throw 'Default region failed.' }
    Assert-Fails { & "$PSScriptRoot/aws-preflight.ps1" -AwsRegion 'eu-west-1' } '*only the us-east-1*'
    $env:AWS_PROFILE = $null
    Assert-Fails { & "$PSScriptRoot/aws-preflight.ps1" } '*Select -AwsProfile*'
    $global:SihPreflightTestFailure = $true
    Assert-Fails { & "$PSScriptRoot/aws-preflight.ps1" -AwsProfile 'test-profile' } '*AWS identity check failed*'
    foreach ($call in $global:SihPreflightTestCalls) {
        if ($call -notmatch '^sts get-caller-identity --profile [^ ]+ --region us-east-1 --output json --no-cli-pager$') {
            throw 'Preflight attempted an unexpected operation.'
        }
    }
} finally {
    $env:AWS_PROFILE = $previousProfile
    $env:AWS_REGION = $previousRegion
    Remove-Item -LiteralPath Function:aws
    Remove-Variable -Scope Global -Name SihPreflightTestCalls, SihPreflightTestFailure
}

$tracked = & git -C $repoRoot ls-files
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect tracked files.' }
foreach ($path in $tracked) {
    if ($path -match '(^|/)\.env\.example$') { continue }
    if ($path -match '(^|/)(node_modules|bin|obj|dist|\.terraform|\.cache|\.tools|artifacts)/|\.tfstate(\.|$)|\.tfplan$|(^|/)\.env($|\.)|\.(pem|key|pfx)$') {
        throw "Forbidden tracked artifact: $path"
    }
}
$terraformRoot = Join-Path $repoRoot 'infra/terraform/environments/lab'
foreach ($file in (Get-ChildItem -LiteralPath $terraformRoot -Filter '*.tf')) {
    if ((Get-Content -LiteralPath $file.FullName -Raw) -match '(?m)^\s*(resource|data|backend)\s+"') {
        throw 'Phase 0 Terraform must contain no resources, data sources or remote backend.'
    }
}
Write-Output 'PowerShell syntax, mocked preflight behavior, tracked-file hygiene and resource-free Terraform checks passed. No AWS call executed.'
