using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationTestStore.Tests.Reports
{
    public static class ExtentTestManager
    {
        private static readonly AsyncLocal<ExtentTest?> _test = new();

        public static ExtentTest GetTest()
        {
            if (_test.Value == null)
                throw new InvalidOperationException("No hay ExtentTest creado para este hilo. Llamá StartTest() primero.");
            return _test.Value!;
        }

        public static void StartTest(string testName)
        {
            var extent = ExtentManager.GetExtent();
            _test.Value = extent.CreateTest(testName);
        }
    }
}