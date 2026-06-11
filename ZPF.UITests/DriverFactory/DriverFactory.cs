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

         if (!success)
         {
            return false;
         }

         client.EndConnect(result);
         return true;
      }
      catch
      {
         return false;
      }
   }

   private static void EnsureAppiumRunning()
   {
      if (IsPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port))
      {
         return;
      }

      StartAppium();

      if (!WaitForPortOpen(UITestViewModel.Current.Config.host, UITestViewModel.Current.Config.port, TimeSpan.FromSeconds(30)))
      {
         throw new InvalidOperationException(
            $"Appium server did not start on {UITestViewModel.Current.Config.host}:{UITestViewModel.Current.Config.port}. "
            + "Start Appium manually and ensure it is reachable before running UI tests.");
      }
   }

   private static bool WaitForPortOpen(string host, int port, TimeSpan timeout)
   {
      var sw = Stopwatch.StartNew();

      while (sw.Elapsed < timeout)
      {
         if (IsPortOpen(host, port))
         {
            return true;
         }

         Thread.Sleep(500);
      }

      return false;
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   public static bool IsRunningEmulator()
   {
      string processName = "emulator";

      return Process.GetProcessesByName(processName).Length > 0;
   }


   private static void StartDroidEmulator()
   {
      var sdkRoot = GetAndroidSdkRoot();
      
      if( OperatingSystem.IsWindows())
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
      else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
      {
         /*
            # Change to the Android SDK directory
            cd ~/Library/Android/sdk/emulator
                        
            # List the available AVDs
            ./emulator -list-avds
                        
            # Launch the chosen AVD
            ./emulator -avd "avd name"
         */
         
         var p = new Process
         {
            StartInfo = new ProcessStartInfo( GetEmulatorPath() )
            {
               Arguments = $"@{UITestViewModel.Current.Config.AndroidDeviceName}",
               UseShellExecute = true
            }
         }.Start();
      }
      else if (OperatingSystem.IsLinux())
      {
         new Process
         {
            StartInfo = new ProcessStartInfo(@"/home/user/Android/Sdk/emulator/emulator")
            {
               Arguments = $"-avd {UITestViewModel.Current.Config.AndroidDeviceName}",
               UseShellExecute = true
            }
         }.Start();
      }
      else
      {
         Debugger.Break(); // Unsupported platform - please implement emulator startup for this OS
      }
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   private static string GetAndroidSdkRoot()
   {
      if (OperatingSystem.IsWindows())
         return @"C:\Program Files (x86)\Android\android-sdk";

      if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
      {
         string[] candidates =
         {
            @$"/Users/{Environment.UserName}/Library/Developer/Xamarin/android-sdk-macosx",
            @"/Users/Shared/Android/sdk"
         };

         foreach (var path in candidates)
         {
            if (Directory.Exists(path))
               return path;
         }

         return candidates[0];
      }

      if (OperatingSystem.IsLinux())
         return @"/home/user/Android/Sdk";

      throw new PlatformNotSupportedException("Unsupported platform for Android SDK lookup.");
   }

   private static string GetEmulatorPath()
   {
      var sdkRoot = GetAndroidSdkRoot();

      if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
      {
         return @$"/Users/{Environment.UserName}/Library/Android/sdk/emulator/emulator";
      }
      
      return OperatingSystem.IsWindows()
         ? Path.Combine(sdkRoot, "emulator", "emulator.exe")
         : Path.Combine(sdkRoot, "emulator", "emulator");
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   private const string AdbPath = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";

   private static string GetAdbPath()
   {
      var sdkRoot = GetAndroidSdkRoot();

      return OperatingSystem.IsWindows()
         ? Path.Combine(sdkRoot, "platform-tools", "adb.exe")
         : Path.Combine(sdkRoot, "platform-tools", "adb");
   }
   
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

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
