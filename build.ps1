param(
    [Parameter(Mandatory=$true)][ValidateSet('Mono','IL2CPP')][string]$Target,
    [Parameter(Mandatory=$true)][string]$XUnityReferenceDirectory,
    [string]$DotnetPath='dotnet'
)
$ErrorActionPreference='Stop'
$dependencies=(Resolve-Path -LiteralPath $XUnityReferenceDirectory).Path
foreach($name in @('XUnity.AutoTranslator.Plugin.Core.dll','XUnity.AutoTranslator.Plugin.ExtProtocol.dll',
    'XUnity.Common.dll','XUnity.ResourceRedirector.dll')) {
    if(!(Test-Path -LiteralPath (Join-Path $dependencies $name))) {throw ('Missing dependency: '+$name)}
}
$configuration=if($Target -eq 'Mono'){'Release'}else{'IL2CPPRelease'}
$project=Join-Path $PSScriptRoot 'SakuraTranslate\SakuraTranslate.csproj'
& $DotnetPath build $project -c $configuration ('-p:XUnityReferenceDirectory='+$dependencies) --nologo
if($LASTEXITCODE -ne 0){throw ('Build failed: '+$LASTEXITCODE)}
