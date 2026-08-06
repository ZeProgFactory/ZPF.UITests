@rem 
@rem  Windows --> Droid
@rem 

@rem Mac $ANDROID_HOME/emulator/emulator -avd pixel_7_-_api_36_0
rem start "" "%ANDROID_HOME%\emulator\emulator.exe" -avd pixel_7_-_api_36_0
rem start "\"%ANDROID_HOME%\emulator\emulator.exe\" -avd pixel_7_-_api_36_0"

dotnet clean Maui\Maui.csproj

msbuild Maui\Maui.csproj -t:Install -p:Configuration=Debug -p:TargetFramework=net10.0-android -p:EnableMauiDevTools=true
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -s emulator-5554 shell am start -n com.companyname.maui/com.companyname.maui.MainActivity

@if %ERRORLEVEL% neq 0 (
    @echo *** Publish failed with error level %ERRORLEVEL% ***
    @exit /b %ERRORLEVEL%
)    

maui devflow init
maui devflow broker start

rem "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -s emulator-5554 reverse tcp:19223 tcp:19223
rem maui devflow wait --device emulator-5554
maui devflow ui tap --automationId ":id/CounterBtn"