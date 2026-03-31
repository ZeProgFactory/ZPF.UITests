using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ZPF.UITests;

public static class AppiumExtensions
{
   public static AppiumElement FindUIElement(this AppiumDriver driver, string id)
   {
      try
      {
         if (driver is WindowsDriver)
         {
            return driver.FindElement(MobileBy.AccessibilityId(id));
         }

         return driver.FindElement(MobileBy.Id(UITestViewModel.Current.Config.PackageID + @":id/" + id));
      }
      catch
      {
         return null;
      }
   }

   public static AppiumElement FindUIElement(this AppiumDriver driver, string id, string text)
   {
      try
      {
         if (driver is WindowsDriver)
         {
            var list = driver.FindElements(MobileBy.AccessibilityId(id));
            var e = list.Where(x => x.Text == text).FirstOrDefault();
            return e;
         }

         {
            var list = driver.FindElements(MobileBy.Id(UITestViewModel.Current.Config.PackageID + @":id/" + id));
            var e = list.Where(x => x.Text == text).FirstOrDefault();
            System.Diagnostics.Debugger.Break();
            return e;
         }
      }
      catch
      {
         return null;
      }
   }

}
