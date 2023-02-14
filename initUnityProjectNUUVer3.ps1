$relativeGamePath = "";

$assetsPath = [System.IO.Path]::Combine($relativeGamePath, "Assets")

if([System.IO.Directory]::Exists(".git") -eq $false) {
    throw "This script must be in root folder of you project and have initialized Github"
}

if ([System.IO.Directory]::Exists($assetsPath) -eq $false) {
    throw """relativeGamePath"" must be set to path contains ""Assets"" unity folder"
}

dotnet nuget add source "https://nuget.twicepricegroup.com/api/Package/fdc0f390-ac03-4981-9208-a4241196ea2c-1aa7e63b-7a53-4e6c-8b47-99bcc56a1a1b-ff706b44-7248-45d9-8de5-736437532bbd/v3/index.json" -n "tp_workload"

$NUUVer3Path = [System.IO.Path]::Combine($assetsPath, "Plugins", "NUUVer3")

git submodule add "https://github.com/Timka654/NUUVer3.git" "$NUUVer3Path"