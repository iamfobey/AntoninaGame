@echo off

pushd %~dp0

echo Installing Pip packages...
pip install pandas openpyxl --quiet
if errorlevel 1 (
    echo Failed to install Pip packages. Ensure pip is correctly installed and configured.
    goto end
)

echo Done.
goto end

:end
pause