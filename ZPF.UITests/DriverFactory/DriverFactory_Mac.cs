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

   public static MacDriver CreateMacDriver()
   {
      if (!IsRunningEmulator())
      {
         StartDroidEmulator();

         Thread.Sleep(TimeSpan.FromSeconds(10)); // Wait for the emulator to fully boot up
      }

      EnsureAppiumRunning();

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
   
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
