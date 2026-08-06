@rem 
@rem  Windows --> Windows
@rem 

dotnet clean Maui\Maui.csproj
start dotnet run --project Maui\Maui.csproj --configuration Debug --framework net10.0-windows10.0.19041.0 --verbosity minimal  

@if %ERRORLEVEL% neq 0 (
    @echo *** Publish failed with error level %ERRORLEVEL% ***
    @exit /b %ERRORLEVEL%
)    

maui devflow init
maui devflow broker start
maui devflow wait
maui devflow ui tap --automationId "CounterBtn"