using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Windows;

namespace MauiApp.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_Android
{
   /// <summary>
   /// MSTest exposes the test result in TestContext.
   /// </summary>
   public TestContext TestContext { get; set; }

   protected AndroidDriver driver;


   [TestInitialize]
   public void Setup()
   {
      driver = DriverFactory.CreateAndroidDriver();
   }

   [TestCleanup]
   public void Cleanup()
   {
      if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
      {
         var testName = TestContext.TestName;

         ScreenshotHelper.Capture(driver, testName, TestContext);
         ScreenshotHelper.CapturePageSource(driver, testName, TestContext);
      }

      driver?.Quit();
   }
}
