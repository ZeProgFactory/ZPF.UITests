using OpenQA.Selenium.Appium;

namespace MauiApp.UITests;

[TestClass]
public class WindowsTests : TestBase_Windows   
{
   [TestMethod]
   public void CounterButton_IncrementsValue()
   {
      var button = driver.FindElement(MobileBy.AccessibilityId("CounterBtn"));

      button.Click();
      button.Click();

      Assert.AreEqual("Clicked 2 times", button.Text);
   }
}
