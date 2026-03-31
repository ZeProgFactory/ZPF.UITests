using ZPF;
using ZPF.UITests;

namespace ZPF.UITests;

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

   string _GetCurentFolder = string.Empty;

   public string GetCurentFolder()
   {
      if (string.IsNullOrEmpty(_GetCurentFolder))
      {

         string result = Config.TestResults;

         if (Config.GroupSessionInFolder)
         {

            switch (Config.FolderNamingStrategy)
            {
               case FolderNamingStrategies.PrevCurrent:
                  result = Path.Join(Config.TestResults, $"Current");

                  if (Directory.Exists(Path.Join(Config.TestResults, $"Previous")))
                  {
                     Directory.Delete(Path.Join(Config.TestResults, $"Previous"), true);
                  }

                  if (Directory.Exists(result))
                  {
                     Directory.Move(result, Path.Join(Config.TestResults, $"Previous"));
                  }

                  break;

               case FolderNamingStrategies.TimeStamp:
                  var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                  result = Path.Join(result, $"{TestContext.TestName}_{timestamp}");
                  break;
            }

            if (!Directory.Exists(result))
            {
               Directory.CreateDirectory(result);
            }
         }

         _GetCurentFolder = result;
         return result;
      }
      else
      {
         return _GetCurentFolder;
      }
   }

   // - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -  - -
}
