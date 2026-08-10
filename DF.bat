@rem 
@rem  Windows --> Droid
@rem 

@rem Mac $ANDROID_HOME/emulator/emulator -avd pixel_7_-_api_36_0
rem start "" "dir %ANDROID_HOME%\emulator\emulator.exe" -avd pixel_7_-_api_36_0
rem start "\"%ANDROID_HOME%\emulator\emulator.exe\" -avd pixel_7_-_api_36_0"

@rem 1. Start the DevFlow broker BEFORE the app so the agent can connect on launch.
maui devflow init
maui devflow broker start

@rem 2. Build and install the Maui app on the emulator.
dotnet clean Maui\Maui.csproj
msbuild Maui\Maui.csproj -t:Install -p:Configuration=Debug -p:TargetFramework=net10.0-android -p:EnableMauiDevTools=true

@if %ERRORLEVEL% neq 0 (
    @echo *** Install failed with error level %ERRORLEVEL% ***
    @exit /b %ERRORLEVEL%
)

@rem 3. Forward the broker port into the emulator so the DevFlow agent can reach it.
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -s emulator-5554 reverse tcp:19223 tcp:19223

@rem 4. Launch the app.
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -s emulator-5554 shell am start -n com.companyname.maui/com.companyname.maui.MainActivity

@rem 5. Wait for the DevFlow agent to connect, then run an action.
maui devflow wait --device emulator-5554
maui devflow ui tap --automationId "CounterBtn"
