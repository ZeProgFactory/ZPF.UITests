using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace MauiApp.UITests;

/// <summary>
/// Handles screenshots + page source capture.
/// </summary>
public static class ScreenshotHelper
{
   public static void Capture(AppiumDriver driver, string testName, TestContext context, bool IsOK = true)
   {
      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var postfix = IsOK ? "" : "_FAIL";
      var folder = string.IsNullOrEmpty(UITestViewModel.Current.Config.TestResults) ? UITestViewModel.Current.TestContext.DeploymentDirectory : UITestViewModel.Current.Config.TestResults;
      var fileName = Path.Join(folder, $"{testName}_{timestamp}{postfix}.png");

      var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
      //screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);
      screenshot.SaveAsFile(fileName);

      context.AddResultFile(fileName);
   }

   public static void CapturePageSource(AppiumDriver driver, string testName, TestContext context)
   {
      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var folder = string.IsNullOrEmpty(UITestViewModel.Current.Config.TestResults) ? UITestViewModel.Current.TestContext.DeploymentDirectory : UITestViewModel.Current.Config.TestResults;
      var fileName = Path.Join(folder, $"{testName}_{timestamp}.xml");

      File.WriteAllText(fileName, driver.PageSource);

      context.AddResultFile(fileName);
   }
}
