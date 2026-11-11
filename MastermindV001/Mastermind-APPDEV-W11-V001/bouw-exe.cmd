@echo off
REM Bouwt Mastermind.exe voor Windows 11 (64-bit).
REM Resultaat: Mastermind-app\Mastermind.exe -- een enkel bestand dat op elke
REM Windows 11-machine draait; .NET hoeft er niet op te staan.

setlocal
set DOTNET=%USERPROFILE%\.dotnet\dotnet.exe
if not exist "%DOTNET%" set DOTNET=dotnet

cd /d "%~dp0"
"%DOTNET%" publish MastermindWin -c Release -r win-x64 --self-contained ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o Mastermind-app
if errorlevel 1 (
  echo.
  echo *** Bouwen mislukt ***
  exit /b 1
)
echo.
echo Klaar: %~dp0Mastermind-app\Mastermind.exe
endlocal
