using OpenQA.Selenium.Appium;

namespace MauiApp.UITests;

[TestClass]
public class AndroidTests : TestBase_Android
{
   [TestMethod]
   public void CounterButton_IncrementsValue()
   {
      // Find elements by MAUI AutomationId
      // var list = driver.FindElements(MobileBy.Id("com.companyname.maui:id/CounterBtn"));
      var button = driver.FindElement(MobileBy.Id("com.companyname.maui:id/CounterBtn"));
      //var button = driver.FindElement(MobileBy.AccessibilityId("CounterBtn"));

      // Click button
      button.Click();
      button.Click();

      // Verify updated text
      Assert.AreEqual("Clicked 2 times", button.Text);
   }
}
