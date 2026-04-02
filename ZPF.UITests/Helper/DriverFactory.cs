using System.Diagnostics;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Internal;
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

         Thread.Sleep(3000);
      }

      // 0. Ensure the device is authorized for ADB debugging
      WaitForDeviceAuthorized();

      // 1. Start Appium if not running
      if (!IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         // Choose one:
         StartAppium();

         // Wait for server to be ready
         Thread.Sleep(4000);
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

   private const string AdbPath = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";

   /// <summary>
   /// Waits for at least one ADB device to be in the "device" (authorized) state.
   /// If the device is in the "unauthorized" state, attempts adb kill-server / start-server
   /// to trigger a new authorization prompt on the device, then retries.
   /// </summary>
   private static void WaitForDeviceAuthorized(int maxRetries = 30, int delayMs = 2000)
   {
      bool killedServer = false;

      for (int i = 0; i < maxRetries; i++)
      {
         var status = GetAdbDeviceStatus();

         if (status == AdbDeviceStatus.Authorized)
         {
            Debug.WriteLine("ADB device is authorized.");
            return;
         }

         if (status == AdbDeviceStatus.Unauthorized && !killedServer)
         {
            // Restart the ADB server to trigger the authorization dialog on the device
            Debug.WriteLine("ADB device unauthorized. Restarting ADB server to trigger authorization prompt...");
            RunAdbCommand("kill-server");
            Thread.Sleep(1000);
            RunAdbCommand("start-server");
            killedServer = true;
         }

         Debug.WriteLine($"Waiting for device authorization (attempt {i + 1}/{maxRetries}, status: {status})...");
         Thread.Sleep(delayMs);
      }

      throw new InvalidOperationException(
         $"Timed out waiting for an authorized ADB device after {maxRetries * delayMs / 1000} seconds. "
         + "Please check the USB debugging authorization dialog on the device/emulator.");
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   private enum AdbDeviceStatus
   {
      NoDevice,
      Unauthorized,
      Authorized
   }

   /// <summary>
   /// Runs "adb devices" and returns the authorization status of the first connected device.
   /// </summary>
   private static AdbDeviceStatus GetAdbDeviceStatus()
   {
      string output = RunAdbCommand("devices");

      // Parse each line after "List of devices attached"
      var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

      foreach (var line in lines)
      {
         var trimmed = line.Trim();

         // Skip header line
         if (trimmed.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            continue;

         // Each device line is: <serial>\t<state>
         if (trimmed.Contains("\t"))
         {
            var parts = trimmed.Split('\t');
            var state = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            if (state.Equals("device", StringComparison.OrdinalIgnoreCase))
               return AdbDeviceStatus.Authorized;

            if (state.Equals("unauthorized", StringComparison.OrdinalIgnoreCase))
               return AdbDeviceStatus.Unauthorized;
         }
      }

      return AdbDeviceStatus.NoDevice;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   /// <summary>
   /// Runs an ADB command and returns the standard output.
   /// </summary>
   private static string RunAdbCommand(string arguments)
   {
      try
      {
         var process = new Process
         {
            StartInfo = new ProcessStartInfo(AdbPath)
            {
               Arguments = arguments,
               UseShellExecute = false,
               RedirectStandardOutput = true,
               RedirectStandardError = true,
               CreateNoWindow = true
            }
         };

         process.Start();
         string output = process.StandardOutput.ReadToEnd();
         process.WaitForExit(10000);

         return output;
      }
      catch (Exception ex)
      {
         Debug.WriteLine($"Error running adb command '{arguments}': {ex.Message}");
         return string.Empty;
      }
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
