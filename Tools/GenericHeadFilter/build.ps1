param([string]$GameRoot = 'K:\KK\kk\Koikatu')
$ErrorActionPreference = 'Stop'
$compiler = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
$managed = Join-Path $GameRoot 'Koikatu_Data\Managed'
$refs = @(
    "$managed\mscorlib.dll", "$managed\System.dll", "$managed\System.Core.dll",
    "$managed\UnityEngine.dll", "$managed\UnityEngine.UI.dll",
    "$managed\Assembly-CSharp.dll", "$managed\Assembly-CSharp-firstpass.dll",
    "$managed\TextMeshPro-1.0.55.56.0b12.dll",
    "$GameRoot\BepInEx\core\BepInEx.dll", "$GameRoot\BepInEx\core\0Harmony.dll",
    "$GameRoot\BepInEx\plugins\KK_BepisPlugins\Sideloader.dll",
    "$GameRoot\BepInEx\plugins\2155X\KK_MakerSearch.dll"
)
$outDir = Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$arguments = @('/noconfig', '/nologo', '/target:library', '/nostdlib+', '/optimize+', '/langversion:7.3', "/out:$outDir\MiunaGenericHeadFilterV101.dll")
$arguments += $refs | ForEach-Object { "/reference:$_" }
$arguments += Join-Path $PSScriptRoot 'MiunaGenericHeadFilter.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $LASTEXITCODE" }
Copy-Item -LiteralPath "$outDir\MiunaGenericHeadFilterV101.dll" -Destination "$outDir\MiunaGenericHeadFilter.dll" -Force
Get-FileHash "$outDir\MiunaGenericHeadFilter.dll" -Algorithm SHA256
