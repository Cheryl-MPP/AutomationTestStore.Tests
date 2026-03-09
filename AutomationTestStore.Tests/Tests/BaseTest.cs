using AutomationTestStore.Tests.Drivers;
using AutomationTestStore.Tests.Reports;
using AutomationTestStore.Tests.Utils;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace AutomationTestStore.Tests.Tests
{
    public abstract class BaseTest
    {
        protected IWebDriver Driver => DriverFactory.Driver;

        private const string BaseUrl = "https://automationteststore.com/";
        private int _stepCounter;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Inicializa reporte una sola vez por clase
            _ = ExtentManager.GetExtent();
        }

        [SetUp]
        public void BaseSetUp()
        {
            // Inicializa driver nuevo para cada test
            DriverFactory.InitDriver();

            // Reinicia contador de pasos
            _stepCounter = 1;

            // Crea el test en el reporte
            ExtentTestManager.StartTest(TestContext.CurrentContext.Test.Name);

            // Ir al home antes de cada test
            Driver.Navigate().GoToUrl(BaseUrl);
        }

        [TearDown]
        public void BaseTearDown()
        {
            var test = ExtentTestManager.GetTest();
            var outcome = TestContext.CurrentContext.Result.Outcome.Status;
            var message = TestContext.CurrentContext.Result.Message;
            var stackTrace = TestContext.CurrentContext.Result.StackTrace;

            try
            {
                if (outcome == TestStatus.Passed)
                {
                    test.Pass("Test completed successfully.");
                }
                else if (outcome == TestStatus.Failed)
                {
                    test.Fail(message);

                    if (!string.IsNullOrWhiteSpace(stackTrace))
                    {
                        test.Fail(stackTrace);
                    }

                    try
                    {
                        var path = ScreenshotHelper.Capture(
                            Driver,
                            TestContext.CurrentContext.Test.Name
                        );

                        Console.WriteLine("SCREENSHOT PATH: " + path);
                        test.AddScreenCaptureFromPath(path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR CAPTURANDO SCREENSHOT: " + ex.Message);
                        test.Warning("No se pudo capturar screenshot: " + ex.Message);
                    }
                }
                else if (outcome == TestStatus.Skipped)
                {
                    test.Skip("Test skipped.");
                }
                else
                {
                    test.Warning("Estado de prueba no identificado.");
                }
            }
            finally
            {
                ExtentManager.Flush();
                DriverFactory.QuitDriver();
            }
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            ExtentManager.Flush();
        }

        protected void LogStep(string message)
        {
            string stepMessage = $"STEP {_stepCounter} - {message}";
            ExtentTestManager.GetTest().Info(stepMessage);
            TestContext.Progress.WriteLine(stepMessage);
            _stepCounter++;
        }

        protected void LogInfo(string message)
        {
            ExtentTestManager.GetTest().Info(message);
            TestContext.Progress.WriteLine(message);
        }

        protected void LogPass(string message)
        {
            ExtentTestManager.GetTest().Pass(message);
            TestContext.Progress.WriteLine(message);
        }

        protected void LogFail(string message)
        {
            ExtentTestManager.GetTest().Fail(message);
            TestContext.Progress.WriteLine(message);
        }
    }
}