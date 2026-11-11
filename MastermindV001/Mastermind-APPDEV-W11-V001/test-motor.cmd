@echo off
REM Controleert de spelregels zonder venster.
REM   test-motor            de controles
REM   test-motor oud        oud en nieuw tellen naast elkaar
REM   test-motor oplos 2000 laat de computer 2000 spellen raden

setlocal
set DOTNET=%USERPROFILE%\.dotnet\dotnet.exe
if not exist "%DOTNET%" set DOTNET=dotnet

cd /d "%~dp0"
if "%1"=="" (
  "%DOTNET%" run --project MastermindTest -c Release -- test
) else (
  "%DOTNET%" run --project MastermindTest -c Release -- %*
)
endlocal
