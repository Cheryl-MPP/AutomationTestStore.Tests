using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationTestStore.Tests.Pages
{
    public class HomePage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        public HomePage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        private By SearchInput => By.Id("filter_keyword");
        private By SearchButton => By.CssSelector("div.button-in-search");
        private By ProductLinks => By.CssSelector(".thumbnail a.prdocutname");
        private By FirstProduct => By.CssSelector(".thumbnail a.prdocutname");

        public HomePage GoTo(string url)
        {
            try
            {
                _driver.Navigate().GoToUrl(url);
                return this;
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine("GoToUrl falló, reintentando... " + ex.Message);

                // Intento de recuperación
                try
                {
                    _driver.Navigate().Refresh();
                }
                catch { /* ignore */ }

                // Reintento 1 vez
                _driver.Navigate().GoToUrl(url);
                return this;
            }
        }

        private By SearchHeading => By.CssSelector("#content h1, .maintext, .heading1");

        public HomePage Search(string text)
        {
            var input = wait.Until(d => d.FindElement(SearchInput));
            input.Clear();
            input.SendKeys(text);

            _driver.FindElement(SearchButton).Click();

            // Esperar a que cargue la página de búsqueda (aunque no haya resultados)
            wait.Until(d => d.Url.Contains("rt=product/search"));

            // Opcional: esperar que aparezca algún header del content (estabilidad)
            var headings = _driver.FindElements(SearchHeading);
            if (headings.Count == 0)
                wait.Until(d => d.FindElements(SearchHeading).Count > 0);

            return this;
        }

        public ProductPage OpenFirstProduct()
        {
            // Esperar a que exista el primer producto
            var first = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(FirstProduct));

            // Hacer scroll para evitar overlay / elemento fuera de vista
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", first);

            // Intentar click normal, si falla usar JS click
            try
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(FirstProduct)).Click();
            }
            catch
            {
                first = _driver.FindElement(FirstProduct);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", first);
            }

            // Esperar navegación a la página del producto (evita quedarse pegado)
            wait.Until(d => d.Url.Contains("product/product") || d.Title.ToLower().Contains("product"));

            return new ProductPage(_driver);
        }

        public bool HasSearchResults()
        {
            var products = _driver.FindElements(By.CssSelector(".thumbnail a.prdocutname"));
            return products.Count > 0;
        }
    }
}