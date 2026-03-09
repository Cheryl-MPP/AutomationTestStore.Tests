using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Threading;

namespace AutomationTestStore.Tests.Pages
{
    public class CheckoutGuestPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        public CheckoutGuestPage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));
        }

        // Página "Account Login" (step antes del guest form)
        private By GuestRadio => By.Id("accountFrm_accountguest");
        private By ContinueAccountBtn => By.CssSelector("button[title='Continue'], input[title='Continue']");

        // Guest form (guest_step_1)
        private By FirstName => By.Id("guestFrm_firstname");
        private By LastName => By.Id("guestFrm_lastname");
        private By Email => By.Id("guestFrm_email");
        private By Telephone => By.Id("guestFrm_telephone");
        private By Address1 => By.Id("guestFrm_address_1");
        private By City => By.Id("guestFrm_city");
        private By Country => By.Id("guestFrm_country_id");
        private By Zone => By.Id("guestFrm_zone_id");
        private By PostCode => By.Id("guestFrm_postcode");

        private By ContinueGuestBtn => By.CssSelector("button[title='Continue'], input[title='Continue'], #checkout_btn");


        public CheckoutConfirmPage FillGuestFormAndContinue()
        {
            wait.Until(d =>
                d.FindElements(GuestRadio).Count > 0 ||
                d.FindElements(FirstName).Count > 0 ||
                d.Url.ToLower().Contains("checkout/confirm") ||
                d.Url.ToLower().Contains("guest_step_3")
            );

            var guestRadio = _driver.FindElements(GuestRadio).FirstOrDefault(e => e.Displayed && e.Enabled);
            if (guestRadio != null)
            {
                try { guestRadio.Click(); }
                catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", guestRadio); }

                var cont = wait.Until(d => d.FindElements(ContinueAccountBtn).FirstOrDefault(e => e.Displayed && e.Enabled));
                if (cont == null)
                    throw new NoSuchElementException("No se encontró botón Continue en Account Login (guest).");

                try { cont.Click(); }
                catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cont); }

                wait.Until(ExpectedConditions.ElementExists(FirstName));
            }

            if (_driver.Url.ToLower().Contains("checkout/confirm") || _driver.Url.ToLower().Contains("guest_step_3"))
                return new CheckoutConfirmPage(_driver);

            string random = Guid.NewGuid().ToString("N")[..6];

            wait.Until(ExpectedConditions.ElementIsVisible(FirstName)).SendKeys("Test");
            _driver.FindElement(LastName).SendKeys("User");
            _driver.FindElement(Email).SendKeys($"test{random}@mail.com");
            _driver.FindElement(Telephone).SendKeys("88888888");
            _driver.FindElement(Address1).SendKeys("Test Address");
            _driver.FindElement(City).SendKeys("San Jose");

            // Country
            var countryElement = wait.Until(ExpectedConditions.ElementToBeClickable(Country));
            var countrySelect = new SelectElement(countryElement);
            countrySelect.SelectByText("Costa Rica");

            // Zone / State con reintentos robustos contra stale o refresh AJAX
            bool zoneSelected = false;

            for (int attempt = 1; attempt <= 5 && !zoneSelected; attempt++)
            {
                try
                {
                    Thread.Sleep(1000);

                    var zoneElement = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(Zone);
                            return (el.Displayed && el.Enabled) ? el : null;
                        }
                        catch (StaleElementReferenceException)
                        {
                            return null;
                        }
                        catch (NoSuchElementException)
                        {
                            return null;
                        }
                    });

                    if (zoneElement == null)
                        continue;

                    var zoneSelect = new SelectElement(zoneElement);

                    if (zoneSelect.Options.Count > 1)
                    {
                        zoneSelect.SelectByIndex(1);
                        zoneSelected = true;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // reintentar
                }
                catch (WebDriverTimeoutException)
                {
                    // reintentar
                }
            }

            if (!zoneSelected)
                throw new WebDriverTimeoutException("No se pudo seleccionar la provincia/estado del guest checkout.");

            _driver.FindElement(PostCode).SendKeys("1000");

            var continueBtn = wait.Until(d => d.FindElements(ContinueGuestBtn).FirstOrDefault(e => e.Displayed && e.Enabled));
            if (continueBtn == null)
                throw new NoSuchElementException("No se encontró botón Continue en Guest Checkout.");

            try { continueBtn.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", continueBtn); }

            wait.Until(d =>
                d.Url.ToLower().Contains("guest_step_3") ||
                d.Url.ToLower().Contains("checkout/confirm")
            );

            return new CheckoutConfirmPage(_driver);
        }
    }
}