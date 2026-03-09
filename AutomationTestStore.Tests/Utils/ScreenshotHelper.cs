using OpenQA.Selenium;

namespace AutomationTestStore.Tests.Utils
{
    public static class ScreenshotHelper
    {
        public static string Capture(IWebDriver driver, string testName)
        {
            var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults", "Screenshots");
            Directory.CreateDirectory(screenshotsDir);

            var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(screenshotsDir, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(filePath);

            return filePath;
        }
    }
}