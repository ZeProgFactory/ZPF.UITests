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
      // UITestViewModel.Current.Config.AndroidDeviceName = "Android Emulator"; // or your device name
      // UITestViewModel.Current.Config.APK = @"C:\Path\To\Your\App.apk";

      UITestViewModel.Current.Config.TestResults = @"D:\GitWare\Nugets\ZPF_UITests\TestResults\";
      UITestViewModel.Current.Config.AndroidDeviceName = "pixel_7_-_api_36_0"; // or your device name
      UITestViewModel.Current.Config.APK = @"D:\GitWare\Nugets\ZPF_UITests\TestApps\com.companyname.maui.apk";
      UITestViewModel.Current.Config.PackageID = "com.companyname.maui";

      UITestViewModel.Current.Config.CompareBeforeAfter = true;
   }


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
   public void _02ChangePage()
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
}
