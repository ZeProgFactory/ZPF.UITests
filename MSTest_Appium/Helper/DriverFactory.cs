using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Windows;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MauiApp.UITests;

public static class DriverFactory
{
   const string DriverUrl = "http://127.0.0.1:4723";

   const string host = "127.0.0.1";
   const int port = 4723;

   const string DeviceName = "pixel_7_-_api_36_0";
   const string APK = @"C:\Users\zepro\AppData\Local\Xamarin\Mono for Android\Archives\2026-03-16\Maui 3-16-26 4.15 PM.apkarchive\com.companyname.maui.apk";
   const string APP = @"D:\GitWare\Apps\Appium\Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Maui.exe";



   public static AndroidDriver CreateAndroidDriver()
   {
      if (!IsRunningEmulator())
      {
         StartEmulator();
      }

      // 1. Start Appium if not running
      if (!IsPortOpen(host, port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(2000);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions();
      options.PlatformName = "Android";
      options.DeviceName = DeviceName;
      options.AutomationName = "UiAutomator2";

      // Path to your MAUI .apk
      options.App = APK;

      // 3. Create session
      var _driver = new AndroidDriver(new Uri(DriverUrl), options);
      _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

      return _driver;
   }


   public static WindowsDriver CreateWindowsDriver()
   {
      // 1. Start Appium if not running
      if (!IsPortOpen(host, port))
      {
         // Choose one:
         StartAppium();
         //StartWinAppDriver();

         // Wait for server to be ready
         Thread.Sleep(2000);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions();
      options.PlatformName = "Windows";
      options.AutomationName = "Windows";
      options.App = APP;

      // 3. Create session
      return new WindowsDriver(new Uri(DriverUrl), options);
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   private static void StartAppium()
   {
      new Process
      {
         StartInfo = new ProcessStartInfo(@"appium")
         {
            Arguments = "--relaxed-security",
            UseShellExecute = true
         }
      }.Start();
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   private static bool IsPortOpen(string host, int port)
   {
      try
      {
         using var client = new System.Net.Sockets.TcpClient();
         var result = client.BeginConnect(host, port, null, null);
         var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(300));
         return success;
      }
      catch
      {
         return false;
      }
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   public static bool IsRunningEmulator()
   {
      string processName = "emulator";

      return Process.GetProcessesByName(processName).Length > 0;
   }


   private static void StartEmulator()
   {
      new Process
      {
         StartInfo = new ProcessStartInfo(@"C:\Program Files (x86)\Android\android-sdk\emulator\emulator.exe")
         {
            Arguments = $"-avd {DeviceName}",
            UseShellExecute = true
         }
      }.Start();
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
