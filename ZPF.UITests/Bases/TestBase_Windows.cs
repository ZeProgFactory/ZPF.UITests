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

      Driver = DriverFactory.CreateWindowsDriver();
      UITestViewModel.Current.TestContext = TestContext;

      if (UITestViewModel.Current.Config.CompareBeforeAfter)
      {
         BeforeImagePath = ScreenshotHelper.Capture(Driver, TestContext, "_BEFORE");
      }
   }
}
