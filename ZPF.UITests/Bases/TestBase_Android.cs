using OpenQA.Selenium.Appium.Android;
using ZPF.UITests;

namespace MauiApp.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_Android : TestBase
{
   [TestInitialize]
   public void Setup()
   {
      Driver = DriverFactory.CreateAndroidDriver();
      UITestViewModel.Current.TestContext = TestContext;

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         BeforeImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_BEFORE");
      }
   }
}
