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
                    var reportDir = Path.Combine(AppContext.BaseDirectory, "TestResults", "ExtentReports");
                    Directory.CreateDirectory(reportDir);

                    var fileName = $"ExtentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                    var reportPath = Path.Combine(reportDir, fileName);

                    var spark = new ExtentSparkReporter(reportPath);

                    spark.Config.DocumentTitle = "Automation Test Report";
                    spark.Config.ReportName = "Selenium NUnit Execution Report";

                    _extent = new ExtentReports();
                    _extent.AttachReporter(spark);

                    _extent.AddSystemInfo("Framework", ".NET + NUnit + Selenium");
                    _extent.AddSystemInfo("Browser", "Chrome");
                    _extent.AddSystemInfo("Environment", "QA");
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