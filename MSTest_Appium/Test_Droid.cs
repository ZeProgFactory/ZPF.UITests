using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using System.Diagnostics;

namespace MauiApp.UITests
{
   [TestClass]
   public class AndroidTests
   {
      public bool IsRunningEmulator()
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
               Arguments = "-avd pixel_7_-_api_36_0",
               UseShellExecute = true
            }
         }.Start();
      }

      private AndroidDriver _driver;

      [TestInitialize]
      public void Setup()
      {
         if (!IsRunningEmulator())
         {
            StartEmulator();
         }


         var options = new AppiumOptions();
         options.PlatformName = "Android";
         options.DeviceName = "pixel_7_-_api_36_0";
         options.AutomationName = "UiAutomator2";

         // Path to your MAUI .apk
         options.App = @"C:\Users\zepro\AppData\Local\Xamarin\Mono for Android\Archives\2026-03-16\Maui 3-16-26 11.18 AM.apkarchive\com.companyname.maui.apk";
         //options.App = @"C:\Users\zepro\AppData\Local\Xamarin\Mono for Android\Archives\2026-03-16\Maui 3-16-26 9.30 AM.apkarchive\com.companyname.maui.apk";
         _driver = new AndroidDriver(new Uri("http://127.0.0.1:4723"), options);
         _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
      }

      [TestCleanup]
      public void Cleanup()
      {
         _driver?.Quit();
      }

      [TestMethod]
      public void CounterButton_IncrementsValue()
      {
         // Find elements by MAUI AutomationId
         var button = _driver.FindElement(MobileBy.AccessibilityId("CounterBtn"));
         //var label = _driver.FindElement(MobileBy.AccessibilityId("CounterLabel"));

         // Initial state
         // Assert.AreEqual("Current count: 0", label.Text);

         // Click button
         button.Click();
         button.Click();

         // Verify updated text
         //Assert.AreEqual("Clicked 2 times", label.Text);
         Assert.AreEqual("Clicked 2 times", button.Text);
      }
   }
}
