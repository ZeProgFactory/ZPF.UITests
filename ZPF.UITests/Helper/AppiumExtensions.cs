using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ZPF.UITests;

public static class AppiumExtensions
{
   public static AppiumElement FindUIElement(this AppiumDriver driver, string id)
   {
      if (driver is WindowsDriver)
      {
         return driver.FindElement(MobileBy.AccessibilityId(id));
      }

      return driver.FindElement(MobileBy.Id(id));
   }
}
