[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$envPath = Join-Path $PSScriptRoot '../infra/compose/.env'
if (Test-Path -LiteralPath $envPath) {
    throw 'The local env file already exists. It was not changed.'
}
$random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $databaseBytes = New-Object byte[] 32
    $brokerBytes = New-Object byte[] 32
    $random.GetBytes($databaseBytes)
    $random.GetBytes($brokerBytes)
    $content = @(
        '# Generated for disposable local development only; do not commit.'
        'POSTGRES_USER=sih_local'
        ('POSTGRES_PASSWORD=' + [Convert]::ToBase64String($databaseBytes))
        'RABBITMQ_USER=sih_local'
        ('RABBITMQ_PASSWORD=' + [Convert]::ToBase64String($brokerBytes))
    ) -join "`n"
    $stream = [System.IO.File]::Open($envPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write)
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($content + "`n")
        $stream.Write($bytes, 0, $bytes.Length)
    } finally {
        $stream.Dispose()
    }
} finally {
    $random.Dispose()
}
Write-Output 'Created infra/compose/.env with random development credentials. Values were not displayed.'
