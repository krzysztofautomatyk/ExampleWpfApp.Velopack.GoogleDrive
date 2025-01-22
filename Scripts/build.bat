@echo off
chcp 1250
setlocal EnableDelayedExpansion

:: Set the file to store the version
set "versionFile=%~dp0version.txt"

:: Check if the file exists, if not - create it with the initial version
if not exist "%versionFile%" (
    echo 1.0.0>"%versionFile%"
)

:: Get the current version from the file
for /f "usebackq tokens=* delims=" %%a in ("%versionFile%") do set "version=%%~a"
echo Current version: %version%

:: Check if the version is in the format x.y.z
for /f "tokens=1-3 delims=." %%a in ("%version%") do (
    set "major=%%a"
    set "minor=%%b"
    set "patch=%%c"
)

:: Increment the patch number by 1
set /a "patch+=1"

:: Create the new version
set "version=%major%.%minor%.%patch%"
echo New version: %version%

:: Save the new version to the file
echo %version%>"%versionFile%"

echo Major: %major%, Minor: %minor%, Patch: %patch%

:: If any segment is empty, it's an error
if "%major%"=="" goto :error
if "%minor%"=="" goto :error
if "%patch%"=="" goto :error

:: Check if `major`, `minor`, and `patch` are numbers
for /l %%n in (0,1,9) do (
    if "%major%"=="%%n" set "isMajorNum=1"
    if "%minor%"=="%%n" set "isMinorNum=1"
    if "%patch%"=="%%n" set "isPatchNum=1"
)
if not defined isMajorNum goto :error
if not defined isMinorNum goto :error
if not defined isPatchNum goto :error
goto :continue

:error
echo ERROR: Invalid version format in version.txt!
exit /b 1

:continue
:: Main script logic
set "output_dir=C:\Projects\ExampleWpfApp.Velopack.GoogleDrive\bin\VeloPack"

echo Compiling ExampleWpfApp.Velopack.GoogleDrive...
dotnet publish -c Release C:\Projects\ExampleWpfApp.Velopack.GoogleDrive\ExampleWpfApp.Velopack.GoogleDrive.csproj   -o "%~dp0publish"

echo Building Velopack Release v%version%...
vpk pack -u ExampleWpfApp.Velopack.GoogleDrive -v "%version%" -o "%~dp0releases" -p "%~dp0publish" -f net8-x64-desktop

echo Copying files to %output_dir%...
if not exist "%output_dir%" mkdir "%output_dir%"
xcopy "%~dp0releases\*" "%output_dir%\" /E /Y /Q

echo Completed successfully!
exit /b 0