using OpenQA.Selenium.Appium.Windows;

namespace MauiApp.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_Windows
{
   /// <summary>
   /// MSTest exposes the test result in TestContext.
   /// </summary>
   public TestContext TestContext { get; set; }

   protected WindowsDriver driver;


   [TestInitialize]
   public void Setup()
   {
      driver = DriverFactory.CreateWindowsDriver();
      UITestViewModel.Current.TestContext = TestContext;
   }

   [TestCleanup]
   public void Cleanup()
   {
      var testName = TestContext.TestName;

      if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
      {
         ScreenshotHelper.Capture(driver, testName, TestContext, false);
         ScreenshotHelper.CapturePageSource(driver, testName, TestContext);
      }
      else
      {
         if (UITestViewModel.Current.Config.ScreenshotOnExit)
         {
            ScreenshotHelper.Capture(driver, testName, TestContext);
         }
      }

      driver?.Quit();
   }
}
