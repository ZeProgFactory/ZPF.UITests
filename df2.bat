@echo off
rem https://copilot.microsoft.com/shares/ZrFejz6tXcKxtSk49D8fk
setlocal

echo === Launching Android emulator (non-blocking) ===
start "" cmd /c "\"%ANDROID_HOME%\emulator\emulator.exe\" -avd pixel_7_-_api_36_0"

echo === Waiting for emulator to appear ===
:wait_device
"%ANDROID_HOME%\platform-tools\adb.exe" devices | findstr "emulator-5554" >nul
if errorlevel 1 (
    echo Emulator not ready yet...
    timeout /t 1 >nul
    goto wait_device
)
echo Emulator detected.

echo === Waiting for Android boot completion ===
:bootcheck
for /f %%i in ('adb -s emulator-5554 shell getprop sys.boot_completed') do set boot=%%i
if "%boot%"=="1" goto bootready
echo Boot not completed...
timeout /t 1 >nul
goto bootcheck

:bootready
echo Android boot completed.

pause

echo === Building MAUI app with DevTools ===
dotnet build -t:Run -f net8.0-android -p:EnableMauiDevTools=true -p:Configuration=Debug
if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo === Locating APK ===
for /f "tokens=*" %%a in ('dir /b /s bin\Debug\net10.0-android\*.apk') do set apk=%%a
echo APK found: %apk%

echo === Installing APK ===
adb -s emulator-5554 install -r "%apk%"

echo === Launching MAUI app ===
adb -s emulator-5554 shell am start -n com.companyname.maui/com.companyname.maui.MainActivity

echo === Setting up DevFlow port forwarding ===
adb -s emulator-5554 reverse tcp:19223 tcp:19223

echo === Verifying DevTools agent ===
adb -s emulator-5554 shell ps | findstr devtools

echo === Attaching DevFlow ===
maui devflow wait --device emulator-5554

echo === Dumping UI tree ===
maui devflow ui tree --device emulator-5554

echo === Script completed ===
endlocal
