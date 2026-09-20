@echo off
cd /d "%~dp0"
dotnet run --project src\Yatzy.App\Yatzy.App.csproj
if errorlevel 1 pause
