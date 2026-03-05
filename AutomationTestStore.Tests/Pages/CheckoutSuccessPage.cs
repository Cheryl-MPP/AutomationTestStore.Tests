using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationTestStore.Tests.Pages
{
    public class CheckoutSuccessPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        public CheckoutSuccessPage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        // Fallbacks típicos
        private By SuccessHeader => By.CssSelector("#content h1, #content h2, .heading1, .maintext");
        private By ContentArea => By.CssSelector("#content");

        public string GetSuccessText()
        {
            // A veces el header cambia, entonces devolvemos lo que haya en content
            var headers = _driver.FindElements(SuccessHeader);
            if (headers.Count > 0)
            {
                var txt = headers[0].Text?.Trim();
                if (!string.IsNullOrEmpty(txt)) return txt;
            }

            // fallback: todo el contenido visible
            var content = wait.Until(d => d.FindElement(ContentArea));
            return content.Text.Trim();
        }
    }
}