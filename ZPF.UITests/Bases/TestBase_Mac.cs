using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using ZPF.UITests;

namespace ZPF.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_Mac : TestBase
{
   [TestInitialize]
   public void Setup()
   {
      Driver = DriverFactory.CreateMacDriver();
      UITestViewModel.Current.TestContext = TestContext;

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         BeforeImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_BEFORE");
      }
   }
}
