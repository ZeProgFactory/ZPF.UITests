using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace ZPF.UITests;

/// <summary>
/// Handles screenshots + page source capture.
/// </summary>
public static class ScreenshotHelper
{
   public static string Capture(AppiumDriver driver, TestContext context, string postfix )
   {
      var testName = context.TestName;

      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var folder = string.IsNullOrEmpty(UITestViewModel.Current.GetCurentFolder()) ? UITestViewModel.Current.TestContext.DeploymentDirectory : UITestViewModel.Current.GetCurentFolder();
      var fileName = Path.Join(folder, $"{testName}_{timestamp}{postfix}.png");

      var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
      //screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);
      screenshot.SaveAsFile(fileName);

      context.AddResultFile(fileName);
      return fileName;
   }

   public static string Capture(AppiumDriver driver, string testName, TestContext context, bool IsOK = true)
   {
      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var postfix = IsOK ? "" : "_FAIL";
      var folder = string.IsNullOrEmpty(UITestViewModel.Current.GetCurentFolder()) ? UITestViewModel.Current.TestContext.DeploymentDirectory : UITestViewModel.Current.GetCurentFolder();
      var fileName = Path.Join(folder, $"{testName}_{timestamp}{postfix}.png");

      var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
      //screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);
      screenshot.SaveAsFile(fileName);

      context.AddResultFile(fileName);
      return fileName;
   }

   public static string CapturePageSource(AppiumDriver driver, string testName, TestContext context)
   {
      var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      var folder = string.IsNullOrEmpty(UITestViewModel.Current.GetCurentFolder()) ? UITestViewModel.Current.TestContext.DeploymentDirectory : UITestViewModel.Current.GetCurentFolder();
      var fileName = Path.Join(folder, $"{testName}_{timestamp}.xml");

      File.WriteAllText(fileName, driver.PageSource);

      context.AddResultFile(fileName);
      return fileName;
   }
}
