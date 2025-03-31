@echo off
setlocal enabledelayedexpansion

FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO RMDIR /S /Q "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

for /r %%f in (*.csproj) do (
    echo Building: %%f
    dotnet build "%%f" --configuration Release
)