using AutomationTestStore.Tests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Linq;

namespace AutomationTestStore.Tests.Pages
{
    public class CategoryPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public CategoryPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        public CategoryPage GoToHome(string baseUrl)
        {
            _driver.Navigate().GoToUrl(baseUrl);
            return this;
        }

        public CategoryPage OpenCategoryByText(string categoryText)
        {
            string target = Normalize(categoryText);

            // selector de links del menú de categorías
            By menuLinks = By.CssSelector("#categorymenu a");

            _wait.Until(d => d.FindElements(menuLinks).Count > 0);

            // Recolectar visibles
            var links = _driver.FindElements(menuLinks)
                .Where(e => e.Displayed)
                .ToList();

            // Debug: imprimir lo que sí existe (para que veas cómo viene)
            Console.WriteLine("CATEGORIAS VISIBLES:");
            foreach (var l in links)
                Console.WriteLine($"- text='{l.Text}' title='{l.GetAttribute("title")}' href='{l.GetAttribute("href")}'");

            IWebElement? match = links.FirstOrDefault(l =>
            {
                var txt = Normalize(l.Text);
                var title = Normalize(l.GetAttribute("title") ?? "");
                var href = (l.GetAttribute("href") ?? "").ToLowerInvariant();

                // match por texto, por title o por URL
                return txt.Contains(target) || title.Contains(target) || href.Contains(target.Replace(" ", ""));
            });

            if (match == null)
                throw new NoSuchElementException($"No se encontró la categoría en el menú: {categoryText}");

            // Hover + click (por si está dentro de dropdown)
            try
            {
                new Actions(_driver).MoveToElement(match).Perform();
            }
            catch { /* ignore */ }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", match);

            try { match.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", match); }

            return this;
        }

        private static string Normalize(string s)
        {
            return (s ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ");
        }

        public ProductPage OpenFirstProductFromCategory()
        {
            // esperar a que carguen productos
            _wait.Until(d => d.FindElements(By.CssSelector(".prdocutname")).Count > 0);

            var products = _driver.FindElements(By.CssSelector(".prdocutname"))
                                  .Where(p => p.Displayed)
                                  .ToList();

            if (!products.Any())
                throw new NoSuchElementException("No se encontró un producto visible dentro de la categoría.");

            var first = products.First();

            var beforeUrl = _driver.Url;
            Console.WriteLine("CATEGORY BEFORE CLICK URL: " + beforeUrl);
            Console.WriteLine("PRODUCT CLICK: " + first.Text);

            // scroll + click más seguro
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", first);

            try { first.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", first); }

            // ✅ esperar navegación real (cambio de URL)
            _wait.Until(d => d.Url != beforeUrl);

            Console.WriteLine("AFTER CLICK URL: " + _driver.Url);
            Console.WriteLine("AFTER CLICK TITLE: " + _driver.Title);

            return new ProductPage(_driver);
        }
    }
}