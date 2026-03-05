using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationTestStore.Tests.Pages
{
    public class CheckoutGuestPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait wait;

        public CheckoutGuestPage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        // Paso intermedio (a veces aparece)
        private By GuestRadio => By.Id("accountFrm_accountguest");
        private By ContinueAccountBtn => By.CssSelector("button[title='Continue'], input[title='Continue']");

        // Form guest
        private By FirstName => By.Id("guestFrm_firstname");
        private By LastName => By.Id("guestFrm_lastname");
        private By Email => By.Id("guestFrm_email");
        private By Telephone => By.Id("guestFrm_telephone");
        private By Address1 => By.Id("guestFrm_address_1");
        private By City => By.Id("guestFrm_city");
        private By Postcode => By.Id("guestFrm_postcode");
        private By Country => By.Id("guestFrm_country_id");
        private By Zone => By.Id("guestFrm_zone_id");
        private By ContinueGuestBtn => By.CssSelector("button[title='Continue'], input[title='Continue']");

        public CheckoutConfirmPage FillGuestFormAndContinue()
        {
            // 1) Si aparece la pantalla para elegir Guest Checkout, la seleccionamos
            var guestOptions = _driver.FindElements(GuestRadio);
            if (guestOptions.Count > 0)
            {
                var guest = wait.Until(d => d.FindElement(GuestRadio));
                if (!guest.Selected) guest.Click();

                wait.Until(d => d.FindElement(ContinueAccountBtn)).Click();
            }

            // 2) Ahora sí esperamos el form de guest y lo llenamos
            wait.Until(d => d.FindElement(FirstName)).SendKeys("Test");
            _driver.FindElement(LastName).SendKeys("User");
            _driver.FindElement(Email).SendKeys($"test{DateTime.Now:HHmmss}@mail.com");
            _driver.FindElement(Telephone).SendKeys("88888888");
            _driver.FindElement(Address1).SendKeys("San Jose");
            _driver.FindElement(City).SendKeys("San Jose");
            _driver.FindElement(Postcode).SendKeys("11501");

            new SelectElement(_driver.FindElement(Country)).SelectByText("Costa Rica");

            // Esperar a que cargue Zone
            wait.Until(d => d.FindElement(Zone));
            var zone = new SelectElement(_driver.FindElement(Zone));
            if (zone.Options.Count > 1) zone.SelectByIndex(1);

            wait.Until(d => d.FindElement(ContinueGuestBtn)).Click();

            return new CheckoutConfirmPage(_driver);
        }

        public bool IsGuestFlowVisible()
        {
            // Si existe el radio guest o el formulario guest, estamos en guest flow
            return _driver.FindElements(By.Id("accountFrm_accountguest")).Count > 0
                || _driver.FindElements(By.Id("guestFrm_firstname")).Count > 0;
        }
    }
}