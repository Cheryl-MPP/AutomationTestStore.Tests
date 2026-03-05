using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;


namespace AutomationTestStore.Tests.Pages
{
    public class CartPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        private readonly By CartTable = By.CssSelector(".cart-info");
        private readonly By CheckoutButton = By.Id("cart_checkout1");

        private By RemoveButtons => By.CssSelector(
            "a[title*='Remove'], a[title*='remove'], " +
            "a[href*='remove'], a[href*='delete'], " +
            "button[title*='Remove'], button[title*='remove'], " +
            "input[value*='Remove'], input[value*='remove'], " +
            "a.btn-remove, a.remove, " +
            "a i.fa-trash, a i.icon-trash"
);
        private By QuantityInputs => By.CssSelector("input[name*='quantity']");

        public CartPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public bool IsCartVisible()
        {
            try
            {
                wait.Until(d => d.FindElement(CartTable).Displayed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public CartPage RemoveFirstItem()
        {
            var waitLocal = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // 1) intentar CSS primero
            IWebElement? remove = null;

            try
            {
                waitLocal.Until(d => d.FindElements(RemoveButtons).Any(e => e.Displayed && e.Enabled));
                remove = driver.FindElements(RemoveButtons).FirstOrDefault(e => e.Displayed && e.Enabled);
            }
            catch { /* ignore */ }

            // 2) fallback XPath: buscar cualquier cosa que diga remove/delete o tenga rt=checkout/cart&remove
            if (remove == null)
            {
                var xpath = "//*[self::a or self::button or self::input]" +
                            "[contains(translate(@title,'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(normalize-space(.),'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(@href,'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(@href,'REMOVEDELETE','removedelete'),'delete') " +
                            "or contains(translate(@value,'REMOVEDELETE','removedelete'),'remove')]";

                remove = driver.FindElements(By.XPath(xpath)).FirstOrDefault(e => e.Displayed && e.Enabled);
            }

            if (remove == null)
            {
                Console.WriteLine("CART URL: " + driver.Url);
                Console.WriteLine("CART TITLE: " + driver.Title);

                throw new NoSuchElementException("No se encontró botón/link Remove/Delete visible en el carrito.");
            }

            // click seguro
            try { remove.Click(); }
            catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", remove); }

            // esperar que ya no haya productos (o que se actualice la página)
            waitLocal.Until(d =>
                d.FindElements(QuantityInputs).Count == 0
                || d.PageSource.ToLower().Contains("empty")
                || d.Url.ToLower().Contains("checkout/cart")
            );

            return this;
        }

        public bool IsCartEmpty()
        {
            // criterio 1: no hay inputs quantity
            var noQtyInputs = driver.FindElements(QuantityInputs).Count == 0;

            // criterio 2: texto de carrito vacío (fallback)
            var html = driver.PageSource.ToLower();
            var hasEmptyText =
                html.Contains("your shopping cart is empty") ||
                html.Contains("shopping cart is empty") ||
                html.Contains("empty");

            return noQtyInputs || hasEmptyText;
        }

        public CheckoutGuestPage ProceedToCheckout()
        {
            var btn = wait.Until(d => d.FindElement(CheckoutButton));
            btn.Click();
            return new CheckoutGuestPage(driver);
        }
    }
}
