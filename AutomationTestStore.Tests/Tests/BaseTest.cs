using AutomationTestStore.Tests.Drivers;
using AutomationTestStore.Tests.Reports;
using AutomationTestStore.Tests.Utils;
using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AutomationTestStore.Tests.Tests
{
    public abstract class BaseTest
    {
        protected IWebDriver Driver => DriverFactory.Driver;

        private const string BaseUrl = "https://automationteststore.com/";

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // 1) Inicializa driver UNA vez por clase (más rápido)
            DriverFactory.InitDriver();

            // 2) Inicializa reporte UNA vez
            _ = ExtentManager.GetExtent();
        }

        [SetUp]
        public void BaseSetUp()
        {
            // Crea el test en el reporte
            ExtentTestManager.StartTest(TestContext.CurrentContext.Test.Name);

            // Reset rápido entre tests para evitar "contaminación"
            try
            {
                Driver.Manage().Cookies.DeleteAllCookies();
            }
            catch { /* por si el driver está en una pantalla rara */ }

            // Volver al home antes de cada test
            try
            {
                Driver.Navigate().GoToUrl(BaseUrl);
            }
            catch { }
        }

        [TearDown]
        public void BaseTearDown()
        {
            var test = ExtentTestManager.GetTest();
            var outcome = TestContext.CurrentContext.Result.Outcome.Status;

            if (outcome == NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                test.Pass("PASS");
            }
            else if (outcome == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                test.Fail(TestContext.CurrentContext.Result.Message);

                try
                {
                    var path = ScreenshotHelper.Capture(Driver, TestContext.CurrentContext.Test.Name);
                    test.AddScreenCaptureFromPath(path);
                }
                catch { }
            }
            else if (outcome == NUnit.Framework.Interfaces.TestStatus.Skipped)
            {
                test.Skip("SKIPPED");
            }

            // OJO: ya NO cerramos driver aquí (para que sea rápido)
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            // Cierra driver una sola vez al final
            DriverFactory.QuitDriver();

            // Flush una sola vez al final
            ExtentManager.Flush();
        }
    }
}