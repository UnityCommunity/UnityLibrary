REM Better version here: https://github.com/appetizermonster/Unity3D-RecompileDisabler

@echo off
REM *** THIS IS NOT WORKING YET ***
REM UnityManualCompiler v0.1
REM Usage: Rename ..Unity\Editor\Data\Mono\bin\mono.exe into xmono.exe (to make auto-compilation fail)
REM Set your own Unity installation folder, MonoPath, TempFile, CurrentDirectory

C:
cd "C:\Program Files\Unity\Editor\Data\Mono\bin\"

"C:\Program Files\Unity\Editor\Data\Mono\bin\xmono.exe" CommandLine="C:\Program Files\Unity\Editor\Data\Mono\lib\mono\unity\smcs.exe  @Temp/UnityTempFile-asdasdasdasdasd", CurrentDirectory="D:/Folder/ProjectA/Assets/.."

REM - TODO Fix missing dll error..

pause
