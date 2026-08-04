![ZPF.UITests](Doc/iconGitHub.svg)

ZPF.UITests is a starter nuget for MSTest and Appium. It provides a simple and efficient way to set up and run UI tests for your applications. With ZPF.UITests, you can quickly create and execute tests that interact with your application's user interface, ensuring that your app works as expected across different devices and platforms.

# !!! under construction !!!

# !!! ??? DevFlow ??? !!!


<PackageReference Include="Microsoft.Maui.DevFlow.Agent" Version="0.1.0-preview.12.26368.2" />

MauiProgram.cs

#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

#if DEBUG
builder.AddMauiDevFlowAgent();
#endif


dotnet tool install -g Microsoft.Maui.Cli --prerelease

maui devflow MAUI tree

maui devflow agent interact tap --automationid "CounterBtn"
