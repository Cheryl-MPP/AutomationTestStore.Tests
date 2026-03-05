using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace AutomationTestStore.Tests.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        private By LoginName => By.Id("loginFrm_loginname");
        private By Password => By.Id("loginFrm_password");
        private By LoginButton => By.CssSelector("button[title='Login']");

        // señales de login exitoso (cualquiera de estas suele existir)
        private By AccountHeading => By.CssSelector("#content h1");
        private By LogoutLink => By.PartialLinkText("Logout");

        public void Login(string username, string password)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(LoginName)).Clear();
            _driver.FindElement(LoginName).SendKeys(username);

            _driver.FindElement(Password).Clear();
            _driver.FindElement(Password).SendKeys(password);

            _wait.Until(ExpectedConditions.ElementToBeClickable(LoginButton)).Click();

            // Espera a que la sesión esté activa (título/heading o link logout)
            _wait.Until(d =>
                d.FindElements(LogoutLink).Count > 0 ||
                (d.FindElements(AccountHeading).Count > 0 && d.FindElement(AccountHeading).Text.ToLower().Contains("account")) ||
                d.Title.ToLower().Contains("account")
            );
        }
    }
}