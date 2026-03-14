using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace MauiApp.UITests
{
   [TestClass]
   public class MainPageTests
   {
      private const string WinAppDriverUrl = "http://127.0.0.1:4723";
      private const string AppPath = @"D:\GitWare\Apps\Appium\Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Maui.exe";

      private static WindowsDriver _session;



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

      private static void StartAppium()
      {
         //var psi = new ProcessStartInfo
         //{
         //   FileName = "appium",
         //   Arguments = "",
         //   UseShellExecute = false,
         //   CreateNoWindow = true
         //};

         //return Process.Start(psi);

         new Process
         {
            StartInfo = new ProcessStartInfo(@"appium")
            {
               UseShellExecute = true
            }
         }.Start();
      }


      [ClassInitialize]
      public static void Setup(TestContext context)
      {
         const string host = "127.0.0.1";
         const int port = 4723;

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
         options.App = AppPath;

         // 3. Create session
         _session = new WindowsDriver(new Uri(WinAppDriverUrl), options);
      }



      //[ClassInitialize]
      //public static void Setup(TestContext context)
      //{
      //   var options = new AppiumOptions();

      //   options.PlatformName = "Windows";
      //   options.AutomationName = "Windows";
      //   options.App = AppPath;

      //   _session = new WindowsDriver(new Uri(WinAppDriverUrl), options);
      //}

      [ClassCleanup]
      public static void TearDown()
      {
         _session?.Quit();
      }

      [TestMethod]
      public void CounterButton_IncrementsValue()
      {
         var button = _session.FindElement(OpenQA.Selenium.Appium.MobileBy.AccessibilityId("CounterBtn"));
         var label = _session.FindElement(OpenQA.Selenium.Appium.MobileBy.AccessibilityId("CounterLabel"));

         button.Click();
         button.Click();

         //Assert.AreEqual("Clicked 2 times", label.Text);
         Assert.AreEqual("Clicked 2 times", button.Text);
      }
   }
}
