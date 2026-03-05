using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AutomationTestStore.Tests.Drivers;


namespace AutomationTestStore.Tests.Drivers
{
    public static class DriverFactory
    {
        private static readonly AsyncLocal<IWebDriver?> _driver = new();

        public static IWebDriver Driver =>
            _driver.Value ?? throw new InvalidOperationException("Driver no inicializado. Llamá InitDriver() primero.");

        public static void InitDriver()
        {
            if (_driver.Value != null) return;

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            // Esto ayuda MUCHO a que no se quede pegado esperando load completo
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            // (Opcional) estabilidad
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(180));

            // Timeouts de Selenium (independientes al command timeout)
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(60);

            _driver.Value = driver;
        }

        public static void QuitDriver()
        {
            try { _driver.Value?.Quit(); } catch { }
            try { _driver.Value?.Dispose(); } catch { }
            _driver.Value = null;
        }
    }
}