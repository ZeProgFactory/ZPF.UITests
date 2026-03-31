using OpenQA.Selenium.Appium;
using ZPF.Skia;

namespace ZPF.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase
{
   /// <summary>
   /// MSTest exposes the test result in TestContext.
   /// </summary>
   public TestContext TestContext { get; set; }


   public string BeforeImagePath { get => _BeforeImagePath; set => _BeforeImagePath = value; }
   string _BeforeImagePath = string.Empty;


   public AppiumDriver Driver { get => _Driver; set => _Driver = value; }
   AppiumDriver _Driver;


   [TestCleanup]
   public void Cleanup()
   {
      var testName = TestContext.TestName;

      if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
      {
         ScreenshotHelper.CapturePageSource(Driver, testName, TestContext);
      }

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         var _AfterImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_AFTER");
         var _DifImagePath = _AfterImagePath.Replace("_AFTER", "_DIF");

         var result = ImageDiff.CompareImages(BeforeImagePath, _AfterImagePath, _DifImagePath, threshold: 15);
         //ToDo: Log the result in a better way, maybe with a custom TestContext extension method that adds it to the test results in a nice format.

         TestContext.AddResultFile(_DifImagePath);
      }
      else
      {
         if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
         {
            ScreenshotHelper.Capture(Driver, testName, TestContext, false);
            ScreenshotHelper.CapturePageSource(Driver, testName, TestContext);
         }
         else
         {
            if (UITestViewModel.Current.Config.ScreenshotOnExit)
            {
               ScreenshotHelper.Capture(Driver, testName, TestContext);
            }
         }
      }

      Driver?.Quit();
   }
}
