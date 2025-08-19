@echo off

pushd %~dp0

set ROOT_DIRECTORY=%~dp0..\..
set TEMP_DIRECTORY=%ROOT_DIRECTORY%\Temp

set PYTHON_VERSION=3.12.5
set PYTHON_URL=https://www.python.org/ftp/python/%PYTHON_VERSION%/python-%PYTHON_VERSION%-amd64.exe
set PYTHON_INSTALLER=%TEMP_DIRECTORY%\python-%PYTHON_VERSION%-amd64.exe

:start
set choice=
set /p choice=Install python-%PYTHON_VERSION%? It is needed for project management. [Y/N/y/n]: 
if not '%choice%'=='' set choice=%choice:~0,1%
if '%choice%'=='Y' goto yes
if '%choice%'=='y' goto yes
if '%choice%'=='N' goto no
if '%choice%'=='n' goto no
if '%choice%'=='' goto start
echo "%choice%" is not valid
echo.
goto start

:yes
echo Checking if %TEMP_DIRECTORY% exists...
if not exist "%TEMP_DIRECTORY%" (
    echo Creating %TEMP_DIRECTORY%...
    mkdir "%TEMP_DIRECTORY%"
    if errorlevel 1 (
        echo Failed to create %TEMP_DIRECTORY%. Ensure you have the necessary permissions.
        goto end
    )
) else (
    echo %TEMP_DIRECTORY% already exists.
)

echo Downloading Python installer from %PYTHON_URL% to %TEMP_DIRECTORY%...
if exist "%PYTHON_INSTALLER%" (
    echo Python installer already exists.
) else (
    powershell -Command "& { (New-Object System.Net.WebClient).DownloadFile('%PYTHON_URL%', '%PYTHON_INSTALLER%') }"
    if errorlevel 1 (
        echo Failed to download the Python installer. Check your internet connection.
        goto end
    )
)

echo Installing Python...
"%PYTHON_INSTALLER%" InstallAllUsers=1 InstallLauncherAllUsers=1 PrependPath=1 CompileAll=1 Include_pip=1 Include_tcltk=0 Include_test=0 Include_doc=0

echo Done.
goto end

:no
echo Skipping Python installation.
goto end

:end
pause