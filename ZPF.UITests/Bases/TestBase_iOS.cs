using OpenQA.Selenium.Appium.iOS;

namespace ZPF.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_iOS : TestBase
{
   [TestInitialize]
   public void Setup()
   {
      Driver = DriverFactory.CreateIOSDriver();
      UITestViewModel.Current.TestContext = TestContext;

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         BeforeImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_BEFORE");
      }
   }
}
