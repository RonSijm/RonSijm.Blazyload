@echo off
setlocal enabledelayedexpansion

set "ProjectName=BLAZYLOAD"
set "MinVersion=[2.0.0-alpha3,)"

:: Don't change things below

cd..

set "CurrentDirectory=%CD%
set "VariableName=USE_!ProjectName!_FROM_SOURCE"
set "PathVariableName=!ProjectName!_PATH"
:: Define paths to include/exclude
set "ExcludePath=Test"
set "IncludePath=src"

:: Loop through all .csproj files in the current directory and subdirectories
for /r %%F in (*.csproj) do (
    set "ProjectFile=%%~F"
    set "ProjectName=%%~nF"
    set "ProjectPath=%%~dpF"
        
    set "SkipFile=0"

    :: Check if file should be excluded (if ProjectPath contains ExcludePath)
    echo !ProjectPath! | findstr /I /C:"!ExcludePath!" >nul && (
        echo Skipping !ProjectFile!
        set "SkipFile=1"
    )
    
    :: Check if file should be included (if IncludePath is set and ProjectPath contains it)
    if not "!IncludePath!"=="" (
        echo !ProjectPath! | findstr /I /C:"!IncludePath!" >nul || (
            echo Skipping !ProjectFile!
            set "SkipFile=1"
        )
    )    

    if "!SkipFile!"=="1" (
        rem Skip this iteration
    ) else (
        set "PropsFile=UseFromSource\References\Include.!ProjectName!.props"
    
        :: Get the relative path by removing the base path from the full path
        set "RelativePath=%%F"
        set "RelativePath=!RelativePath:%CurrentDirectory%\=!"
    
        echo ^<Project^> > !PropsFile!
        echo   ^<ItemGroup Condition="'$(!VariableName!)' ^!= 'true'"^> >> !PropsFile!
        echo     ^<PackageReference Include="!ProjectName!" Version="!MinVersion!" /^> >> !PropsFile!
        echo   ^</ItemGroup^> >> !PropsFile!
        echo. >> !PropsFile!
        echo   ^<ItemGroup Condition="'$(!VariableName!)' == 'true'"^> >> !PropsFile!
        echo     ^<ProjectReference Include="$(!PathVariableName!)^\!RelativePath!" /^> >> !PropsFile!
        echo   ^</ItemGroup^> >> !PropsFile!
        echo ^</Project^> >> !PropsFile!
    
        echo Created !PropsFile!
    )
)

endlocal
