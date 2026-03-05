using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationTestStore.Tests.Reports
{
    public static class ExtentManager
    {
        private static readonly object _lock = new();
        private static ExtentReports? _extent;

        public static ExtentReports GetExtent()
        {
            if (_extent != null) return _extent;

            lock (_lock)
            {
                if (_extent == null)
                {
                    var reportDir = Path.Combine(AppContext.BaseDirectory, "TestResults");
                    Directory.CreateDirectory(reportDir);

                    var reportPath = Path.Combine(reportDir, "ExtentReport.html");

                    var spark = new ExtentSparkReporter(reportPath);

                    _extent = new ExtentReports();
                    _extent.AttachReporter(spark);

                    _extent.AddSystemInfo("Framework", ".NET + NUnit + Selenium");
                    _extent.AddSystemInfo("Browser", "Chrome");
                }
            }

            return _extent!;
        }

        public static void Flush()
        {
            _extent?.Flush();
        }
    }
}