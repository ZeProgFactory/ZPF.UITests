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

   public static AndroidDriver CreateAndroidDriver()
   {
      if (!IsRunningEmulator())
      {
         StartDroidEmulator();

         Thread.Sleep(3000);
      }

      // 0. Ensure the device is authorized for ADB debugging
      WaitForDeviceAuthorized();

      EnsureAppiumRunning();

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
      };

      if (!string.IsNullOrEmpty(UITestViewModel.Current.Config.APK))
      {
         // RELEASE BUILD SETUP
         // The full path to the .apk file
         // This only works with release builds because debug builds have fast deployment enabled
         // and Appium isn't compatible with fast deployment

         // The full path to the .apk file to test or the package name if the app is already installed on the device

         options.App = UITestViewModel.Current.Config.APK;

         // END RELEASE BUILD SETUP
      }
      else
      {
         // DEBUG BUILD SETUP
         // If you're running your tests against debug builds you'll need to set NoReset to true
         // otherwise appium will delete all the libraries used for Fast Deployment on Android
         // Release builds have Fast Deployment disabled
         // https://learn.microsoft.com/xamarin/android/deploy-test/building-apps/build-process#fast-deployment
         options.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");
         options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, UITestViewModel.Current.Config.PackageID );

         //Make sure to set [Register("com.companyname.basicappiumsample.MainActivity")] on the MainActivity of your android application
         options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, $"{UITestViewModel.Current.Config.PackageID}.MainActivity");
         // END DEBUG BUILD SETUP
      }

      options.DeviceName = UITestViewModel.Current.Config.AndroidDeviceName;

            // 3. Create session
      var _driver = new AndroidDriver(new Uri(UITestViewModel.Current.Config.DriverUrl), options);
      _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

      return _driver;
   }
   
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
