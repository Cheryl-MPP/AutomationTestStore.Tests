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
            var beforeUrl = _driver.Url;

            // 1) Marcar términos si existen
            TryCheckTerms();

            // 2) Click al botón confirm (más específico primero)
            IWebElement? btn = TryFind(ConfirmBtn1) ?? TryFind(ConfirmBtn2);

            if (btn == null)
                throw new NoSuchElementException("No se encontró el botón de Confirm Order en checkout confirmation.");

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

            // Intento 1: click normal
            try { btn.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }

            // 3) Esperar a que realmente cambie de pantalla (URL o título)
            var changed = WaitForCheckoutToFinish(beforeUrl);

            if (!changed)
            {
                // Si no cambió, reintentar con submit (a veces el form necesita submit)
                try { btn.Submit(); } catch { /* ignore */ }

                changed = WaitForCheckoutToFinish(beforeUrl);
            }

            if (!changed)
            {
                // debug útil para vos y el tech lead
                throw new WebDriverTimeoutException(
                    $"Se intentó confirmar orden pero no cambió de pantalla. URL={_driver.Url} TITLE={_driver.Title}"
                );
            }

            return new CheckoutSuccessPage(_driver);
        }

        private void TryCheckTerms()
        {
            var cb = _driver.FindElements(TermsCheckbox1).FirstOrDefault()
                     ?? _driver.FindElements(TermsCheckbox2).FirstOrDefault();

            if (cb != null && !cb.Selected)
            {
                try { cb.Click(); }
                catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cb); }
            }
        }

        private IWebElement? TryFind(By by)
        {
            var els = _driver.FindElements(by);
            return els.Count > 0 ? els[0] : null;
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