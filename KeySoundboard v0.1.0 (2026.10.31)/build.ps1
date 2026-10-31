$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
$refDir = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8"
$out = Join-Path $root "KeySoundboard.exe"
$manifest = Join-Path $root "app.manifest"

if (-not (Test-Path $csc)) { throw "找不到 Roslyn 编译器：$csc" }
if (-not (Test-Path $refDir)) { throw "找不到 .NET Framework 4.8 引用程序集：$refDir" }
if (-not (Test-Path $manifest)) { throw "找不到 app.manifest" }

$sources = Get-ChildItem -Path (Join-Path $root "src") -Recurse -Filter "*.cs" |
    Sort-Object FullName |
    ForEach-Object { $_.FullName }

if (-not $sources -or $sources.Count -eq 0) { throw "src 目录下没有 C# 源文件" }

$references = @(
    "mscorlib.dll",
    "System.dll",
    "System.Core.dll",
    "System.Drawing.dll",
    "System.Windows.Forms.dll",
    "System.Xml.dll",
    "System.Xaml.dll",
    "WindowsBase.dll",
    "PresentationCore.dll"
)

$arguments = @(
    "/nologo",
    "/target:winexe",
    "/platform:anycpu",
    "/langversion:latest",
    "/codepage:65001",
    "/optimize+",
    "/debug-",
    "/nostdlib+",
    "/utf8output",
    "/out:$out",
    "/win32manifest:$manifest"
)

foreach ($name in $references) {
    $path = Join-Path $refDir $name
    if (-not (Test-Path $path)) { throw "缺少引用程序集：$path" }
    $arguments += "/reference:$path"
}

$arguments += $sources

Write-Host "编译 $($sources.Count) 个源文件 -> $out"
& $csc $arguments
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败，退出码 $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "编译成功：$out" -ForegroundColor Green
Get-Item $out | Select-Object FullName, Length, LastWriteTime | Format-List
