@echo off

python --version >nul 2>&1
if %errorlevel% neq 0 (
    goto errorNoPython
)

pip --version >nul 2>&1
if %errorlevel% neq 0 (
    goto errorNoPython
)

call :CheckPackage openpyxl
if %errorlevel% neq 0 (
    goto errorNoPackages
)

call :CheckPackage pandas
if %errorlevel% neq 0 (
    goto errorNoPackages
)

goto :havePython

:havePython
python Scripts/Update.py
goto :end

:CheckPackage
set package_name=%1
pip show %package_name% >nul 2>&1
if %errorlevel% neq 0 (
    exit /b 1
)
exit /b 0

:errorNoPython
cd "Scripts/BatchFiles/"
start /wait cmd /c call "GetPython.bat"
cd ../../
goto :retryCheck

:errorNoPackages
cd "Scripts/BatchFiles/"
start /wait cmd /c call "GetPyPackages.bat"
cd ../../
goto :retryCheck

:retryCheck
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python not installed
    goto :end
)

call :CheckPackage openpyxl
if %errorlevel% neq 0 (
    echo pip openpyxl not installed
    goto :end
)

call :CheckPackage pandas
if %errorlevel% neq 0 (
    echo pip pandas not instsalled
    goto :end
)

goto :havePython

:end
popd
rmdir /S /Q "./Temp/" 2>nul
pause