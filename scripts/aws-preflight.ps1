[CmdletBinding()]
param(
    [string]$AwsProfile = $env:AWS_PROFILE,
    [string]$AwsRegion = $(if ($env:AWS_REGION) { $env:AWS_REGION } else { 'us-east-1' })
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    throw 'AWS CLI is not installed. Install AWS CLI v2 before running the identity preflight.'
}
if ([string]::IsNullOrWhiteSpace($AwsProfile)) {
    throw 'Select -AwsProfile or set AWS_PROFILE. The documented local example is new-profile-name.'
}
if ($AwsRegion -ne 'us-east-1') {
    throw 'This project currently permits only the us-east-1 AWS region.'
}

# The only AWS operation in this script is a read-only identity request.
$identityJson = & aws sts get-caller-identity --profile $AwsProfile --region $AwsRegion --output json --no-cli-pager
if ($LASTEXITCODE -ne 0) {
    throw 'AWS identity check failed. Authenticate locally using aws login with the selected profile, then retry.'
}
$identity = $identityJson | ConvertFrom-Json
if (-not $identity.Account -or -not $identity.Arn) {
    throw 'AWS STS returned an incomplete identity response.'
}
[PSCustomObject]@{
    Account = $identity.Account
    Identity = $identity.Arn
    Profile = $AwsProfile
    Region = $AwsRegion
}
