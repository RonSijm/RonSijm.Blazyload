@echo off
setlocal enabledelayedexpansion

set "PREFIX=RonSijm.Blazyload"

set "NUGET_CACHE=%USERPROFILE%\.nuget\packages"

if "%PREFIX%"=="" (
    echo Usage: clear-nuget-cache.bat PackagePrefix
    exit /b 1
)

echo Searching for packages starting with "%PREFIX%" in "%NUGET_CACHE%"...

for /d %%D in ("%NUGET_CACHE%\%PREFIX%*") do (
    echo Deleting %%D...
    rmdir /s /q "%%D"
)

FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO RMDIR /S /Q "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

for /r %%f in (*.csproj) do (
    echo Building: %%f
    dotnet build "%%f" --configuration Release
)