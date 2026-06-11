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
   
   public static WindowsDriver CreateWindowsDriver()
   {
      EnsureAppiumRunning();

      // 2. Configure Appium options
      var options = new AppiumOptions();
      options.PlatformName = "Windows";
      options.AutomationName = "Windows";
      options.App = UITestViewModel.Current.Config.APP_WIN;

      // 3. Create session
      return new WindowsDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);
   }
   
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
