using System.Text.Json;

namespace AutomationTestStore.Tests.Config
{
    public class TestConfig
    {
        public string BaseUrl { get; set; } = "";

        private static TestConfig? _instance;

        public static TestConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    var json = File.ReadAllText("testsettings.json");
                    _instance = JsonSerializer.Deserialize<TestConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })!;
                }

                return _instance;
            }
        }
    }
}