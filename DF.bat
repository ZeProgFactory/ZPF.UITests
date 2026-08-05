
dotnet clean Maui\Maui.csproj
@rem dotnet run --project Maui\Maui.csproj --configuration Debug --framework net10.0-windows10.0.19041.0 --verbosity minimal  
dotnet build --project Maui\Maui.csproj --configuration Debug -f net10.0-windows10.0.19041.0 -t:Run

@if %ERRORLEVEL% neq 0 (
    @echo *** Publish failed with error level %ERRORLEVEL% ***
    @exit /b %ERRORLEVEL%
)    

maui devflow init
maui devflow broker start
maui devflow wait
maui devflow ui tap --automationId "CounterBtn"