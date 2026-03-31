using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace ZPF.UITests;

public static class DriverFactory
{
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   public static MacDriver CreateMacDriver()
   {
      if (!IsRunningEmulator())
      {
         StartEmulator();

         Thread.Sleep(2500);
      }

      // 1. Start Appium if not running
      if (!IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(3000);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions
      {
         // Always Mac for Mac
         PlatformName = "Mac",

         // Specify mac2 as the driver, typically don't need to change this
         AutomationName = "mac2",

         // The full path to the .app file to test
         App = UITestViewModel.Current.Config.APP_OSX,
      };

      // Setting the Bundle ID is required, else the automation will run on Finder
      options.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, UITestViewModel.Current.Config.BundleID_OSX);


      // 3. Create session
      var _driver = new MacDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);

      return _driver;
   }


   public static IOSDriver CreateIOSDriver()
   {
      if (!IsRunningEmulator())
      {
         StartEmulator();

         Thread.Sleep(2500);
      }

      // 1. Start Appium if not running
      if (!IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(3000);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions
      {
         // Specify XCUITest as the driver, typically don't need to change this
         AutomationName = "XCUITest",

         // Always iOS for iOS
         PlatformName = "iOS",

         // iOS Version
         PlatformVersion = "17.0",

         // Don't specify if you don't want a specific device
         DeviceName = UITestViewModel.Current.Config.iOSDeviceName,

         // The full path to the .app file to test or the bundle id if the app is already installed on the device
         App = UITestViewModel.Current.Config.APP_iOS,
      };

      // 3. Create session
      var _driver = new IOSDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);

      return _driver;
   }


   public static AndroidDriver CreateAndroidDriver()
   {
      if (!IsRunningEmulator())
      {
         StartEmulator();

         Thread.Sleep(2500);
      }

      // 1. Start Appium if not running
      if (!IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(3000);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions
      {
         // Always Android for Android
         PlatformName = "Android",

         // Specify UIAutomator2 as the driver, typically don't need to change this
         AutomationName = "UIAutomator2",

         // This is the Android version, not API level
         // This is ignored if you use the avd option below
         // PlatformVersion = "13",

         // The full path to the .apk file to test or the package name if the app is already installed on the device
         App = "com.companyname.basicappiumsample",
      };

      options.DeviceName = UITestViewModel.Current.Config.AndroidDeviceName;

      // Path to your MAUI .apk
      options.App = UITestViewModel.Current.Config.APK;

      // 3. Create session
      var _driver = new AndroidDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);
      _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

      return _driver;
   }


   public static WindowsDriver CreateWindowsDriver()
   {
      // 1. Start Appium if not running
      if (!IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(2500);
      }

      // 2. Configure Appium options
      var options = new AppiumOptions();
      options.PlatformName = "Windows";
      options.AutomationName = "Windows";
      options.App = UITestViewModel.Current.Config.APP_WIN;

      // 3. Create session
      return new WindowsDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);
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
            Arguments = $"-avd {UITestViewModel.Current.Config.AndroidDeviceName}",
            UseShellExecute = true
         }
      }.Start();
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
