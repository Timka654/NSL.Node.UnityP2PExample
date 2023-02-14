Example project already processed this

For new project in current time NSL.Node have format "source code" and need

- make sure you have PowerShell 7.3.x+ or install latest version
- copy scripts "initUnityProjectNode.ps1" and "initUnityProjectNUUVer3.ps1" to you Unity repository root path
- if you "Assets" folder in current(root directory relative repo) - not need, if not - you must change "$relativeGamePath" variable value to relative path you folder contains "Assets"
- (var. 1)execute script "initUnityProjectNUUVer3.ps1" for install plugin manage Nuget
- (var. 1)open project and search/install packages (with title context menu "NuGet" -> "NuGet Package Manager")
-- Unity.NSL.BuilderExtensions.SocketCore.Unity
-- Unity.NSL.BuilderExtensions.SocketCore
-- Unity.NSL.BuilderExtensions.UDPClient
-- Unity.NSL.BuilderExtensions.UDPServer
-- Unity.NSL.BuilderExtensions.WebSocketsClient.Unity
-- Unity.NSL.BuilderExtensions.WebSocketsServer
- (var. 2) clone https://github.com/Timka654/NSL.git to external dir
- (var. 2) build project with BuildReleaseAll.ps1 script
- (var. 2) copy output "build/Release/unity_dll_xxxx" to you "Assets/Plugins/NSL"
- execute script "initUnityProjectNode.ps1" for add submodules