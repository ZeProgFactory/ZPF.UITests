using System.Drawing;
using OpenQA.Selenium.Appium;

namespace ZPF.UITests;

public static class AppiumExtensions_Android
{
   #region - - - APP - - -

   public static bool IsAppInstalled(this AppiumDriver driver)
   {
      try
      {
         var args = new Dictionary<string, object>
         {
             { "appId", UITestViewModel.Current.Config.PackageID }
         };

         return (bool)driver.ExecuteScript("mobile: isAppInstalled", args);
      }
      catch
      {
         return false;
      }
   }


   public static bool TerminateApp(this AppiumDriver driver)
   {
      try
      {
         var args = new Dictionary<string, object>
         {
             { "appId", UITestViewModel.Current.Config.PackageID }
         };

         driver.ExecuteScript("mobile: terminateApp", args);

         return true;
      }
      catch
      {
         return false;
      }
   }


   public static bool ActivateApp(this AppiumDriver driver)
   {
      try
      {
         var args = new Dictionary<string, object>
         {
             { "appId", UITestViewModel.Current.Config.PackageID }
         };

         driver.ExecuteScript("mobile: activateApp", args);

         return true;
      }
      catch
      {
         return false;
      }
   }

   #endregion

   #region - - - DeviceInfo - - -
   /*
    
androidId: 6ae86982e7010435
apiVersion: 36
bluetooth: System.Collections.Generic.Dictionary`2[System.String,System.Object]
brand: google
carrierName: T-Mobile
displayDensity: 420
locale: en_US
manufacturer: Google
model: sdk_gphone64_x86_64
networks: System.Collections.ObjectModel.ReadOnlyCollection`1[System.Object]
platformVersion: 16
realDisplaySize: 1080x2400
timeZone: Europe/Paris

    */

   static Dictionary<string, object> dicoDeviceInfo = null;

   public static int DisplayDensity(this AppiumDriver driver)
   {
      try
      {
         if (dicoDeviceInfo == null)
         {
            dicoDeviceInfo = driver.ExecuteScript("mobile: deviceInfo") as Dictionary<string, object>;
         }

         var result = (long)dicoDeviceInfo.Where(x => x.Key == "displayDensity").FirstOrDefault().Value;

         return (int)result;
      }
      catch
      {
         return -1;
      }
   }

   public struct Rect
   {
      public int Width { get; set; }
      public int Height { get; set; }

      public static Rect Null { get => new Rect(-1, -1); }

      public Rect(int width, int height)
      {
         Width = width;
         Height = height;
      }
   }

   public static Rect DisplaySize(this AppiumDriver driver)
   {
      try
      {
         if (dicoDeviceInfo == null)
         {
            dicoDeviceInfo = driver.ExecuteScript("mobile: deviceInfo") as Dictionary<string, object>;
         }

         var val = (string)dicoDeviceInfo.Where(x => x.Key == "realDisplaySize").FirstOrDefault().Value;

         var r = val.Split('x');

         return new Rect(int.Parse(r[0]), int.Parse(r[1]));

      }
      catch
      {
         return Rect.Null;
      }
   }

   #endregion

}
