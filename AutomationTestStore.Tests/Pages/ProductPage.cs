using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace AutomationTestStore.Tests.Pages
{
    public class ProductPage
    {

        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        private By ProductName => By.CssSelector("h1[itemprop='name']");
        private By AddToCartButton => By.CssSelector("a.cart");

        public ProductPage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public string GetName()
        {
            return wait.Until(d => d.FindElement(ProductName)).Text.Trim();
        }

        public CartPage AddToCart()
        {
            var button = wait.Until(d => d.FindElement(AddToCartButton));
            button.Click();
            return new CartPage(_driver);
        }

    }
}
