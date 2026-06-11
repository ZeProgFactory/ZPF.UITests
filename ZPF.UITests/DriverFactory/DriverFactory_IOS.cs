using System.Diagnostics;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
//using OpenQA.Selenium.Internal;
//using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace ZPF.UITests;

public static partial class DriverFactory
{
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
   
   public static IOSDriver CreateIOSDriver()
   {
      if (!IsRunningEmulator())
      {
         StartDroidEmulator();

         Thread.Sleep(2500);
      }

      EnsureAppiumRunning();

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
   
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
