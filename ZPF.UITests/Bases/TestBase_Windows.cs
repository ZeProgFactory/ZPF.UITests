using System.ComponentModel;
using System.Diagnostics;

namespace ZPF.UITests;

/// <summary>
/// Screenshots on failure & Page source on failure
/// </summary>
[TestClass]
public class TestBase_Windows : TestBase
{
   [TestInitialize]
   public void Setup()
   {
      // 3)

      if (!System.IO.File.Exists(UITestViewModel.Current.Config.APP_WIN))
      {
         // ($"Missing file Config.APP_WIN: {UITestViewModel.Current.Config.APP_WIN}");
         Debugger.Break();
      }

      Driver = DriverFactory.CreateWindowsDriver();
      UITestViewModel.Current.TestContext = TestContext;

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         BeforeImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_BEFORE");
      }
   }
}
