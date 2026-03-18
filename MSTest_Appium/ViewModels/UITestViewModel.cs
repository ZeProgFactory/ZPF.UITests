using ZPF;

namespace MauiApp.UITests;

/// <summary>
/// Singleton instance of the ViewModel, can be used in test classes to access configuration and TestContext.
/// </summary>
public class UITestViewModel : BaseViewModel<UITestViewModel>
{
   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -

   /// <summary>
   /// Configuration for the UI tests, can be set in the test class constructor or in a [ClassInitialize] method.
   /// </summary>
   public UITestConfig Config { get; set; } = new UITestConfig();

   /// <summary>
   /// MSTest exposes the test result in TestContext.
   /// </summary>
   public TestContext TestContext { get; set; }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
