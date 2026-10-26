param(
    [Parameter(Mandatory=$true)][string]$DllPath,
    [Parameter(Mandatory=$true)][string]$XUnityReferenceDirectory,
    [string]$GoldenDllPath
)
$ErrorActionPreference='Stop'
$dependencies=(Resolve-Path -LiteralPath $XUnityReferenceDirectory).Path
$dll=(Resolve-Path -LiteralPath $DllPath).Path
$output=Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$exe=Join-Path $output 'VerifyEndpoint.exe'
& $compiler /nologo /reference:System.Web.Extensions.dll ('/reference:'+(Join-Path $dependencies 'XUnity.AutoTranslator.Plugin.Core.dll')) ('/out:'+$exe) (Join-Path $PSScriptRoot 'VerifyEndpoint.cs')
if($LASTEXITCODE -ne 0){throw 'Verification program failed to compile.'}
[IO.File]::WriteAllText(($exe+'.config'),'<configuration><runtime><loadFromRemoteSources enabled="true" /></runtime></configuration>')
$currentReport=Join-Path $output 'current.json'
& $exe $dll $dependencies $currentReport
if($LASTEXITCODE -ne 0){throw 'Verification failed.'}
if($GoldenDllPath) {
    $golden=(Resolve-Path -LiteralPath $GoldenDllPath).Path
    $goldenReport=Join-Path $output 'golden.json'
    & $exe $golden $dependencies $goldenReport
    if($LASTEXITCODE -ne 0){throw 'Golden DLL verification failed.'}
    if([IO.File]::ReadAllText($currentReport) -cne [IO.File]::ReadAllText($goldenReport)) {
        throw 'Prompt, dictionary, conversion or fake API results differ from the golden DLL.'
    }
    Write-Output 'Golden DLL behavior comparison passed.'
}
