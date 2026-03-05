using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationTestStore.Tests.Pages
{
    public class CheckoutConfirmPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        public CheckoutConfirmPage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
        }

        // Check de términos (puede variar según template)
        private By TermsCheckbox1 => By.Id("agree");
        private By TermsCheckbox2 => By.CssSelector("input[type='checkbox'][name*='agree'], input[type='checkbox'][id*='agree']");

        // Botón confirmar (varía)
        private By ConfirmBtn1 => By.CssSelector("#checkout_btn_confirm");
        private By ConfirmBtn2 => By.CssSelector("button[title='Confirm Order'], input[title='Confirm Order'], a[title='Confirm Order']");

        public CheckoutSuccessPage ConfirmOrder()
        {
            // 1) Asegurarnos que estamos en confirm (guest/auth)
            wait.Until(d =>
                d.Url.ToLower().Contains("guest_step_3") ||
                d.Url.ToLower().Contains("checkout/confirm") ||
                d.Title.ToLower().Contains("checkout confirmation")
            );

            Console.WriteLine("BEFORE CONFIRM URL: " + _driver.Url);
            Console.WriteLine("BEFORE CONFIRM TITLE: " + _driver.Title);

            var beforeUrl = _driver.Url;

            // 2) Aceptar términos si aparece
            TryCheckTerms();

            // 3) Esperar a que exista ALGÚN botón de confirm (sin morir en el primer selector)
            IWebElement? btn = null;

            var found = wait.Until(d =>
            {
                btn =
                    d.FindElements(ConfirmBtn1).FirstOrDefault(e => e.Displayed && e.Enabled) ??
                    d.FindElements(ConfirmBtn2).FirstOrDefault(e => e.Displayed && e.Enabled) ??
                    d.FindElements(By.Id("checkout_btn")).FirstOrDefault(e => e.Displayed && e.Enabled) ??
                    d.FindElements(By.CssSelector("button[title*='Confirm'], input[title*='Confirm'], a[title*='Confirm']"))
                     .FirstOrDefault(e => e.Displayed && e.Enabled);

                return btn != null;
            });

            if (!found || btn == null)
                throw new NoSuchElementException("No se encontró el botón de Confirm Order en checkout confirmation.");

            // 4) Scroll + click seguro
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

            try { btn.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }

            // 5) Esperar que termine checkout (success)
            var ok = WaitForCheckoutToFinish(beforeUrl);
            Assert.That(ok, Is.True, "Se hizo click en Confirm, pero no navegó a Success.");

            return new CheckoutSuccessPage(_driver);
        }

        private void TryCheckTerms()
        {
            var cb = TryFind(TermsCheckbox1) ?? TryFind(TermsCheckbox2);

            if (cb != null && !cb.Selected)
            {
                try { cb.Click(); }
                catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cb); }
            }
        }

        private IWebElement? TryFind(By by)
        {
            return _driver.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
        }

        private bool WaitForCheckoutToFinish(string beforeUrl)
        {
            try
            {
                return wait.Until(d =>
                    d.Url != beforeUrl &&
                    (d.Url.Contains("checkout/success") || d.Title.ToLower().Contains("success") || d.PageSource.ToLower().Contains("your order"))
                );
            }
            catch
            {
                return false;
            }
        }
    }
}