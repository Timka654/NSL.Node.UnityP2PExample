$relativeGamePath = "";

$assetsPath = [System.IO.Path]::Combine($relativeGamePath, "Assets")

if([System.IO.Directory]::Exists(".git") -eq $false) {
    throw "This script must be in root folder of you project and have initialized Github"
}

if ([System.IO.Directory]::Exists($assetsPath) -eq $false) {
    throw """relativeGamePath"" must be set to path contains ""Assets"" unity folder"
}

$gameServerPath = [System.IO.Path]::Combine($relativeGamePath, "GameServer")

git submodule add "https://github.com/Timka654/NSL.Node.git" "$gameServerPath"


$networkPath = [System.IO.Path]::Combine($assetsPath, "Scripts", "Network")


$nodeRoomPluginPath = [System.IO.Path]::Combine($networkPath, "NodeRoomPlugin")

git submodule add "https://github.com/Timka654/NSL.Node.Unity.git" "$nodeRoomPluginPath"


$nodeCorePath = [System.IO.Path]::Combine($networkPath, "Node", "Core")

git submodule add "https://github.com/Timka654/NSL.Node.RoomSharedFiles.git" "$nodeCorePath"

$initCommand = "$nodeCorePath/initialize.ps1"

& $initCommand