namespace ZPF.UITests;

public enum FolderNamingStrategies { PrevCurrent, TimeStamp }

public class UITestConfig
{
   /// <summary>
   /// If empty 'TestContext.DeploymentDirectory' is used, else the specified path is used for screenshots and page source capture.
   /// </summary>
   public string TestResults { get; set; } = @"D:\GitWare\Apps\Appium\TestResults\";


   public bool GroupSessionInFolder { get; set; } = true;

   public FolderNamingStrategies FolderNamingStrategy { get; set; } = FolderNamingStrategies.PrevCurrent;


   /// <summary>
   /// If true, a screenshot will be taken on test exit even if the test passed. Useful for visual verification of the final state. 
   /// </summary>
   public bool ScreenshotOnExit { get; set; } = true;


   public bool CompareBeforeAfter { get; set; } = false;

   public bool CapturePageSource { get; set; } = false;


   #region - - - Appium Server Configuration - - -

   public string DriverUrl { get; set; } = "http://127.0.0.1:4723";

   public string host { get; set; } = "127.0.0.1";
   public int port { get; set; } = 4723;

   #endregion


   #region - - - Windows - - -

   public string APP_WIN { get; set; } = @"D:\GitWare\Apps\Appium\Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Maui.exe";

   #endregion


   #region - - - Android - - -

   public string AndroidDeviceName { get; set; } = "pixel_7_-_api_36_0";
   public string APK { get; set; } = @"C:\Users\zepro\AppData\Local\Xamarin\Mono for Android\Archives\2026-03-16\Maui 3-16-26 9.30 AM.apkarchive\com.companyname.maui.apk";
   //public string APK { get; set; } = @"C:\Users\zepro\AppData\Local\Xamarin\Mono for Android\Archives\2026-03-16\Maui 3-16-26 4.15 PM.apkarchive\com.companyname.maui.apk";

   #endregion


   #region - - - iOS - - -

   public string iOSDeviceName { get; set; } = "iPhone 15 Pro";
   public string APP_iOS { get; set; } = "the path for the app";

   #endregion


   #region - - - Mac - - -

   public string BundleID_OSX { get; set; } = "com.companyname.theapp";
   public string APP_OSX { get; set; } = "/path/to/TheApp.app";
   public string PackageID { get; set; }

   #endregion

}
