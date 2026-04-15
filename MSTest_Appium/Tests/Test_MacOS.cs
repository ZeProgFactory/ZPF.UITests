using System.Drawing.Printing;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using ZPF.UITests;

namespace MauiApp.UITests;

[TestClass]
public class MacTests : TestBase_Mac
{
   public MacTests() : base()
   {
      // Constructor for this test class

      // 2)
   }


   // Runs once before all tests in this class
   [ClassInitialize]
   public static void ClassInit(TestContext context)
   {
      // Global setup for this test class

      // 1)
      UITestViewModel.Current.Config.TestResults = @"/Volumes/Data/Gitware/Nugets/UITests/TestResults/Mac/";
      UITestViewModel.Current.Config.BundleID_OSX = "com.companyname.maui";
      UITestViewModel.Current.Config.APP_OSX = "/Volumes/Data/Gitware/Nugets/UITests/Maui/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/Maui.app";
            
      UITestViewModel.Current.Config.GroupSessionInFolder = true;
      UITestViewModel.Current.Config.FolderNamingStrategy = FolderNamingStrategies.PrevCurrent;
      UITestViewModel.Current.Config.CompareBeforeAfter = true;
      UITestViewModel.Current.Config.CapturePageSource = true;
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
   public void _02CounterButton_IncrementsValue()
   {
      Task.Delay(5000).Wait(); // Wait for the click to register and show up on the screenshot

      var width = 1906;
      var height = 1016;
      var startX = width / 2;
      var startY = 730;

      // https://deepwiki.com/appium/appium-uiautomator2-driver/3.2-gesture-commands
      // Tap at specific coordinates 
      var args = new Dictionary<string, object>
      {
         { "x", startX },
         { "y", startY }
      };

      Driver.ExecuteScript("windows: click", args);
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

      Driver.ExecuteScript("windows: click", args);
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

      var button = Driver.FindUIElement("CounterBtn");
      Assert.AreEqual("Clicked 2 times", button.Text);
   }


   [TestMethod]
   public void _03ChangePage()

   {
      Driver.FindUIElement("MenuBar").Click();
      var m = Driver.FindUIElement("navItem", "Home09");
      m.Click();
      Task.Delay(50).Wait(); // Wait for the click to register and show up on the screenshot

      var appTitle = Driver.FindUIElement("AppTitle").Text;
      var pageTitle = Driver.FindUIElement("title").Text;

      Assert.AreEqual("Home09", pageTitle);
   }
}
