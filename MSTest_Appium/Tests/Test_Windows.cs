using System.Drawing.Printing;
using OpenQA.Selenium.Appium;
using ZPF.UITests;

namespace MauiApp.UITests;

[TestClass]
public class WindowsTests : TestBase_Windows
{
   public WindowsTests() : base()
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
      UITestViewModel.Current.Config.TestResults = @"D:\GitWare\Nugets\ZPF_UITests\TestResults\Win\";
      UITestViewModel.Current.Config.APP_WIN = @"D:\GitWare\Nugets\ZPF_UITests\Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Maui.exe";
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
   public void _02ChangePage()
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
