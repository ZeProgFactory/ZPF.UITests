using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using ZPF.UITests;

namespace MauiApp.UITests;

[TestClass]
public class AndroidTests : TestBase_Android
{
   // Runs once before all tests in this class
   [ClassInitialize]
   public static void ClassInit(TestContext context)
   {
      // Global setup for this test class

      // 1)
      UITestViewModel.Current.Config.TestResults = @"D:\GitWare\Nugets\ZPF_UITests\TestResults\Droid\";
      UITestViewModel.Current.Config.AndroidDeviceName = "pixel_7_-_api_36_0"; // or your device name
      //UITestViewModel.Current.Config.APK = @"D:\GitWare\Nugets\ZPF_UITests\TestApps\com.companyname.maui.apk";
      UITestViewModel.Current.Config.PackageID = "com.companyname.maui";

      UITestViewModel.Current.Config.GroupSessionInFolder = true;
      UITestViewModel.Current.Config.FolderNamingStrategy = FolderNamingStrategies.PrevCurrent;
      UITestViewModel.Current.Config.CompareBeforeAfter = true;
      UITestViewModel.Current.Config.CapturePageSource = true;
   }

   /// <summary>
   /// Execute methods available in the current driver version are: mobile: getDeviceTime, mobile: getNotifications, 
   /// mobile: deviceidle, mobile: getGeolocation, mobile: getContexts, mobile: getPerformanceData, 
   /// mobile: getDisplayDensity, mobile: setGeolocation, mobile: resetGeolocation, mobile: deviceInfo, mobile: getPermissions, 
   /// mobile: deleteFile, mobile: getConnectivity, mobile: getUiMode, mobile: getSystemBars, mobile: getCurrentPackage, 
   /// mobile: getActionHistory, mobile: getClipboard, mobile: performEditorAction, mobile: startActivity, 
   /// mobile: getChromeCapabilities, mobile: setConnectivity, mobile: setUiMode, mobile: gsmVoice, 
   /// mobile: getCurrentActivity, mobile: getAppStrings, mobile: viewportRect, mobile: acceptAlert, 
   /// mobile: openNotifications, mobile: scheduleAction, mobile: unscheduleAction, mobile: setClipboard, 
   /// mobile: listDisplays, mobile: startScreenStreaming, mobile: stopScreenStreaming, mobile: queryAppState, 
   /// mobile: activateApp, mobile: removeApp, mobile: terminateApp, mobile: startService, mobile: stopService, 
   /// mobile: refreshGpsCache, mobile: hideKeyboard, mobile: sendTrimMemory, mobile: fingerprint, mobile: gsmSignal, 
   /// mobile: networkSpeed, mobile: sensorSet, mobile: clickGesture, mobile: longClickGesture, mobile: swipeGesture, 
   /// mobile: scrollBackTo, mobile: deepLink, mobile: dismissAlert, mobile: batteryInfo, mobile: replaceElementValue, 
   /// mobile: screenshots, mobile: shell, mobile: stopLogsBroadcast, mobile: changePermissions, mobile: pushFile, 
   /// mobile: pullFile, mobile: installApp, mobile: clearApp, mobile: broadcast, mobile: isLocked, mobile: bluetooth, 
   /// mobile: getPerformanceDataTypes, mobile: toggleGps, mobile: statusBar, mobile: gsmCall, mobile: powerAc, 
   /// mobile: powerCapacity, mobile: flingGesture, mobile: doubleClickGesture, mobile: pinchOpenGesture, mobile: scroll, 
   /// mobile: type, mobile: installMultipleApks, mobile: pressKey, mobile: resetAccessibilityCache, mobile: listWindows, 
   /// mobile: startLogsBroadcast, mobile: listSms, mobile: pullFolder, mobile: isAppInstalled, mobile: listApps, 
   /// mobile: backgroundApp, mobile: lock, mobile: unlock, mobile: isKeyboardShown, mobile: nfc, mobile: isGpsEnabled, 
   /// mobile: sendSms, mobile: dragGesture, mobile: pinchCloseGesture, mobile: scrollGesture, mobile: viewportScreenshot, 
   /// mobile: execEmuConsoleCommand, mobile: startMediaProjectionRecording, mobile: stopMediaProjectionRecording, 
   /// mobile: injectEmulatorCameraImage, mobile: isMediaProjectionRecordingRunning'
   /// </summary>


   [TestMethod]
   public void _01CounterButton_IncrementsValue()
   {
      var button = Driver.FindUIElement("CounterBtn");

      button.Click();
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot
      button.Click();
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

      Assert.AreEqual("Clicked 2 times", button.Text);
   }


   [TestMethod]
   public void _02CounterButton_IncrementsValue()
   {
      Task.Delay(5000).Wait(); // Wait for the click to register and show up on the screenshot

      var width = Driver.DisplaySize().Width;    // 1080
      var height = Driver.DisplaySize().Height;  // 2400

      var startX = width / 2;
      var startY = 1790;                         // 1790 

      var displayDensity = Driver.DisplayDensity(); // 420


      if( Driver.IsAppInstalled() )
      {
         Driver.TerminateApp();
      }

      if (Driver.ActivateApp())
      {
         Task.Delay(4000).Wait(); // Wait for the UI to update
      }
      else
      {
      };


      {
         // https://deepwiki.com/appium/appium-uiautomator2-driver/3.2-gesture-commands
         // Tap at specific coordinates 
         var args = new Dictionary<string, object>
         {
            { "x", startX },
            { "y", startY }
         };

         Driver.ExecuteScript("mobile: clickGesture", args);
         Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

         Driver.ExecuteScript("mobile: clickGesture", args);
         Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot
      }

      var button = Driver.FindUIElement("CounterBtn");
      Assert.AreEqual("Clicked 2 times", button.Text);
   }


   [TestMethod]
   public void _03ChangePage()
   {
      var m0 = Driver.FindElement(MobileBy.AccessibilityId("Open navigation drawer"));
      m0.Click();
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

      var m1 = Driver.FindUIElement("shellHome09");
      m1.Click();
      Task.Delay(500).Wait(); // Wait for the click to register and show up on the screenshot

      // var pageTitle = Driver.FindElement(By.XPath("//androidx.appcompat.widget.Toolbar//android.widget.TextView")).Text;

      //// Wait for the page to load and the toolbar to be present
      //var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
      //var pageTitle = wait.Until(d => 
      //    d.FindElement(By.XPath("//androidx.appcompat.widget.Toolbar//android.widget.TextView"))
      //).Text;

      //var appTitle = Driver.FindUIElement("AppTitle").Text;
      // var pageTitle = Driver.

      Assert.AreEqual("Home09", "toto");
   }



   [TestMethod]
   public void _10AppInstall()
   {
      if (Driver.IsAppInstalled())
      {
         Driver.TerminateApp();
         Task.Delay(1000).Wait(); // Wait for the UI to update

         Driver.RemoveApp();
         Task.Delay(4000).Wait(); // Wait for the UI to update
      }

      UITestViewModel.Current.Config.APK = @"D:\GitWare\Nugets\ZPF_UITests\TestApps\com.companyname.maui.apk";

      Driver.InstallApp(UITestViewModel.Current.Config.APK);
      Task.Delay(2000).Wait(); // Wait for the UI to update

      Assert.AreEqual(true, Driver.IsAppInstalled());
   }


}
