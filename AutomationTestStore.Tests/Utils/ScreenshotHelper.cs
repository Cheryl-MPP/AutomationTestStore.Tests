using OpenQA.Selenium;

namespace AutomationTestStore.Tests.Utils
{
    public static class ScreenshotHelper
    {
        public static string Capture(IWebDriver driver, string fileNamePrefix = "screenshot")
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "TestResults", "Screenshots");
            Directory.CreateDirectory(dir);

            var fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var path = Path.Combine(dir, fileName);

            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(path);

            return path;
        }
    }
}