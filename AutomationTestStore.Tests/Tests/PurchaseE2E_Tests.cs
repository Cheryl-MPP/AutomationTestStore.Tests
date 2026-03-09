using AutomationTestStore.Tests.Config;
using AutomationTestStore.Tests.Pages;
using AutomationTestStore.Tests.Reports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Buffers.Text;


namespace AutomationTestStore.Tests.Tests
{
    [NonParallelizable]// dado que algunos tests hacen limpieza de procesos, es mejor no correr en paralelo para evitar conflictos. Si se quiere paralelizar, habría que aislar aún más el driver y la limpieza.
    [TestFixture]// esta clase es para tests de compra E2E, pero también incluye algunos tests relacionados a navegación, registro y carrito para aprovechar el setup común. Se pueden mover a otras clases si se quiere.
    public class PurchaseE2E_Tests : BaseTest
    {

        [Test]
        [Category("Purchase")]
        public void Purchase_Product_GuestCheckout_ShouldSucceed()
        {
            LogStep("Search product 'shampoo' from homepage");

            var home = new HomePage(Driver!)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            LogInfo("URL: " + Driver!.Url);
            LogInfo("TITLE: " + Driver!.Title);

            LogStep("Open first product from search results");

            var product = home.OpenFirstProduct();

            LogStep("Add product to cart");

            var cart = product.AddToCart();

            LogStep("Validate cart is visible");

            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");

            LogPass("Cart displayed successfully.");

            LogStep("Proceed to checkout");

            var checkout = cart.ProceedToCheckout();

            LogStep("Fill guest checkout form");

            var confirm = checkout.FillGuestFormAndContinue();

            LogInfo("BEFORE CONFIRM URL: " + Driver!.Url);
            LogInfo("BEFORE CONFIRM TITLE: " + Driver!.Title);

            LogStep("Confirm order");

            var success = confirm.ConfirmOrder();

            LogInfo("AFTER CONFIRM URL: " + Driver!.Url);
            LogInfo("AFTER CONFIRM TITLE: " + Driver!.Title);

            LogStep("Validate checkout confirmation step finished");

            Assert.That(
                Driver!.Url,
                Does.Not.Contain("guest_step_3"),
                "Se quedó en Checkout Confirmation; no se completó la orden."
            );

            LogStep("Validate success URL");

            Assert.That(
                Driver!.Url.ToLowerInvariant(),
                Does.Contain("success")
                    .Or.Contain("checkout/success")
                    .Or.Contain("guest_step_4")
                    .Or.Contain("guest_step_5"),
                "No se detectó navegación a página de éxito (revisar URL)."
            );

            LogPass("Success URL detected.");

            LogStep("Validate success message on page");

            var successText = success.GetSuccessText().ToLowerInvariant();

            Assert.That(
                successText,
                Does.Contain("success")
                .Or.Contain("your order")
                .Or.Contain("checkout success")
                .Or.Contain("thank you"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Guest checkout completed successfully.");
        }

        //Falso negativo : sabemos que no hay productos con ese nombre,
        //pero validamos que sí existan resultados (aunque no existan)
        [Test]
        [Category("Negative-Demo")]
        public void Search_ProductNotFound_ShouldFail()
        {
            LogStep("Open Automation Test Store homepage");

            var home = new HomePage(Driver!)
                .GoTo(TestConfig.Instance.BaseUrl);

            LogStep("Search for a non-existing product");

            home = home.Search("lamborghini12345");

            LogInfo("NEGATIVE URL: " + Driver!.Url);
            LogInfo("NEGATIVE TITLE: " + Driver!.Title);

            LogStep("Validate search results");

            var hasResults = home.HasSearchResults();

            LogStep("Force failure to validate reporting and screenshot");

            Assert.That(hasResults, Is.True,
                "Escenario negativo: no se encontraron productos para la búsqueda.");
        }

        [Test]
        [Category("Register")]
        [Category("NewUser")]
        public void Register_NewUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");
            Driver!.Navigate().GoToUrl("https://automationteststore.com/");

            LogStep("Open login or register page");
            Driver.FindElement(By.LinkText("Login or register")).Click();

            LogStep("Click continue to open registration form");
            Driver.FindElement(By.CssSelector("button[title='Continue']")).Click();

            LogStep("Wait for registration form to be visible");
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElements(By.Id("AccountFrm_firstname")).Count > 0);

            LogStep("Generate random user data to avoid duplicates");
            string random = Guid.NewGuid().ToString("N").Substring(0, 6);

            LogStep("Fill registration form");
            Driver.FindElement(By.Id("AccountFrm_firstname")).SendKeys("Test");
            Driver.FindElement(By.Id("AccountFrm_lastname")).SendKeys("User");
            Driver.FindElement(By.Id("AccountFrm_email")).SendKeys($"test{random}@mail.com");
            Driver.FindElement(By.Id("AccountFrm_telephone")).SendKeys("88888888");
            Driver.FindElement(By.Id("AccountFrm_address_1")).SendKeys("Test Address");
            Driver.FindElement(By.Id("AccountFrm_city")).SendKeys("San Jose");

            LogStep("Select country");
            var countryElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("AccountFrm_country_id")));
            var countrySelect = new SelectElement(countryElement);
            countrySelect.SelectByText("Costa Rica");

            LogStep("Wait for province/state dropdown to refresh");
            wait.Until(d =>
            {
                try
                {
                    var zone = d.FindElement(By.Id("AccountFrm_zone_id"));
                    var options = zone.FindElements(By.TagName("option"));
                    return zone.Displayed && zone.Enabled && options.Count > 1;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });

            LogStep("Select province/state");
            var zoneElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("AccountFrm_zone_id")));
            var zoneSelect = new SelectElement(zoneElement);
            zoneSelect.SelectByIndex(1);

            Driver.FindElement(By.Id("AccountFrm_postcode")).SendKeys("1000");
            Driver.FindElement(By.Id("AccountFrm_loginname")).SendKeys($"user{random}");
            Driver.FindElement(By.Id("AccountFrm_password")).SendKeys("Password123");
            Driver.FindElement(By.Id("AccountFrm_confirm")).SendKeys("Password123");

            LogStep("Accept terms and conditions");
            Driver.FindElement(By.Id("AccountFrm_agree")).Click();

            LogStep("Submit registration");
            Driver.FindElement(By.CssSelector("button[title='Continue']")).Click();

            LogStep("Wait for successful navigation to account page");
            wait.Until(d =>
                d.Url.ToLower().Contains("account") ||
                d.Title.ToLower().Contains("account"));

            LogInfo("FINAL URL: " + Driver.Url);
            LogInfo("FINAL TITLE: " + Driver.Title);

            LogStep("Validate user was redirected to account page");
            Assert.That(
                Driver.Url.ToLower(),
                Does.Contain("account"),
                "No se navegó a la página de cuenta después del registro."
            );

            LogPass("User registration completed successfully.");
        }

        [Test]
        [Category("Login")]
        [Category("OldUser")]
        public void Login_RegisteredUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");

            Driver!.Navigate().GoToUrl("https://automationteststore.com/");

            LogStep("Open login page");

            Driver.FindElement(By.LinkText("Login or register")).Click();

            LogStep("Enter registered username");

            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");

            LogStep("Enter password");

            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");

            LogStep("Click login button");

            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            LogStep("Validate successful login");

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se detectó la página de cuenta después del login."
            );

            LogPass("Registered user login completed successfully.");
        }

        [Test]
        [Category("Purchase")]
        [Category("OldUser")]
        public void Purchase_Product_AuthenticatedUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");
            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);

            LogStep("Open login page");
            Driver.FindElement(By.LinkText("Login or register")).Click();

            LogStep("Enter registered username");
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");

            LogStep("Enter password");
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");

            LogStep("Click login button");
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            LogStep("Wait for successful login");
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            wait.Until(d =>
                d.Url.ToLower().Contains("account") ||
                d.Title.ToLower().Contains("account"));

            LogInfo("LOGIN URL: " + Driver.Url);
            LogInfo("LOGIN TITLE: " + Driver.Title);

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");

            LogStep("Search for product 'shampoo'");
            var home = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            LogStep("Open first product");
            var product = home.OpenFirstProduct();

            LogStep("Add product to cart");
            var cart = product.AddToCart();

            LogStep("Validate cart is visible");
            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");

            LogPass("Cart is visible.");

            LogStep("Proceed to checkout");
            var checkout = cart.ProceedToCheckout();

            LogStep("Continue checkout flow");

            CheckoutConfirmPage confirm;

            try
            {
                confirm = checkout.FillGuestFormAndContinue();
                LogInfo("Guest form appeared and was completed.");
            }
            catch (Exception ex)
            {
                LogInfo("Guest form did not appear. Continuing directly to confirmation page. " + ex.Message);
                confirm = new CheckoutConfirmPage(Driver!);
            }

            LogInfo("AUTH BEFORE CONFIRM URL: " + Driver.Url);
            LogInfo("AUTH BEFORE CONFIRM TITLE: " + Driver.Title);

            LogStep("Confirm order");
            var success = confirm.ConfirmOrder();

            LogInfo("AUTH AFTER CONFIRM URL: " + Driver.Url);
            LogInfo("AUTH AFTER CONFIRM TITLE: " + Driver.Title);

            LogStep("Validate success URL");

            Assert.That(
                Driver.Url.ToLowerInvariant(),
                Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito."
            );

            LogPass("Success URL detected.");

            LogStep("Validate success message");

            var successText = success.GetSuccessText().ToLowerInvariant();

            Assert.That(
                successText,
                Does.Contain("your order").Or.Contain("processed").Or.Contain("success"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Order completed successfully.");
        }

        [Test]
        [Category("PurchaseMultiple")]
        [Category("OldUser")]
        public void Purchase_MultipleProducts_AuthenticatedUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");

            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);

            LogStep("Login with registered user");

            Driver.FindElement(By.LinkText("Login or register")).Click();
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");

            LogStep("Search and add first product 'shampoo'");

            var home = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            var product1 = home.OpenFirstProduct();
            var cart = product1.AddToCart();

            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se ve después del primer producto"
            );

            LogPass("First product added to cart.");

            LogStep("Search and add second product 'cream'");

            var home2 = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("cream");

            var product2 = home2.OpenFirstProduct();
            cart = product2.AddToCart();

            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se ve después del segundo producto"
            );

            LogPass("Second product added to cart.");

            LogStep("Proceed to checkout");

            cart.ProceedToCheckout();

            LogInfo("MULTI AUTH CHECKOUT URL: " + Driver.Url);
            LogInfo("MULTI AUTH CHECKOUT TITLE: " + Driver.Title);

            LogStep("Confirm order");

            var confirm = new CheckoutConfirmPage(Driver);

            var success = confirm.ConfirmOrder();

            LogInfo("MULTI AUTH AFTER CONFIRM URL: " + Driver.Url);
            LogInfo("MULTI AUTH AFTER CONFIRM TITLE: " + Driver.Title);

            LogStep("Validate success URL");

            Assert.That(
                Driver.Url.ToLowerInvariant(),
                Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito."
            );

            LogStep("Validate success message");

            var successText = success.GetSuccessText().ToLowerInvariant();

            Assert.That(
                successText,
                Does.Contain("your order")
                .Or.Contain("processed")
                .Or.Contain("success"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Multiple product purchase completed successfully.");
        }

        [Test]
        [Category("Cart")]
        [Category("OldUser")]
        public void Cart_UpdateQuantity_RemoveProduct_ValidateTotals_ShouldSucceed()
        {
            LogStep("Login with registered user");

            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);

            Driver.FindElement(By.LinkText("Login or register")).Click();
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");

            LogStep("Search product and add to cart");

            var home = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            var product = home.OpenFirstProduct();
            var cart = product.AddToCart();

            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se ve después de agregar producto."
            );

            LogPass("Product added to cart.");

            LogStep("Open Shopping Cart page");

            Driver.Navigate().GoToUrl($"{TestConfig.Instance.BaseUrl}/index.php?rt=checkout/cart");

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("shopping cart").Or.Contain("cart"),
                "No abrió Shopping Cart."
            );

            LogStep("Read initial cart total");

            decimal ReadTotal()
            {
                var candidates = Driver.FindElements(By.XPath(
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::span[contains(@class,'price')][1] | " +
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::td[1]"
                ));

                var raw = candidates.FirstOrDefault()?.Text ?? "";

                if (string.IsNullOrWhiteSpace(raw))
                {
                    var prices = Driver.FindElements(By.CssSelector("span.price"));
                    raw = prices.LastOrDefault()?.Text ?? "";
                }

                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var val))
                    return val;

                if (decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        new System.Globalization.CultureInfo("es-CR"),
                        out val))
                    return val;

                Console.WriteLine("No se pudo parsear TOTAL. RAW=" + raw);
                return -1m;
            }

            var totalBefore = ReadTotal();
            LogInfo("TOTAL BEFORE: " + totalBefore);

            LogStep("Update quantity to 2");

            var qtyInput = Driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                .FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(qtyInput, Is.Not.Null, "No se encontró input de cantidad en el carrito.");

            qtyInput!.Clear();
            qtyInput.SendKeys("2");

            LogStep("Click Update button");

            var updateBtn = Driver.FindElements(By.XPath(
                "//button[contains(.,'Update') or contains(@title,'Update')] | " +
                "//input[@value='Update' or contains(@title,'Update')] | " +
                "//a[contains(.,'Update')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(updateBtn, Is.Not.Null, "No se encontró botón Update en el carrito.");

            updateBtn!.Click();

            LogStep("Wait for quantity update");

            var waitQty = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));

            waitQty.Until(d =>
            {
                try
                {
                    var input = d.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                        .FirstOrDefault(e => e.Displayed);

                    if (input == null) return false;

                    return input.GetAttribute("value") == "2";
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });

            LogStep("Read updated total");

            decimal TryReadCartTotal()
            {
                var totalEl = Driver.FindElements(By.CssSelector(
                    ".total .price, #totals_table .price, .cart_total .price, .cart-info .price, span.price"
                )).LastOrDefault();

                var raw = totalEl?.Text ?? "";

                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var val))
                    return val;

                return -1m;
            }

            var totalAfterUpdate = TryReadCartTotal();

            LogInfo("TOTAL AFTER UPDATE: " + totalAfterUpdate);

            if (totalBefore >= 0 && totalAfterUpdate >= 0)
            {
                Assert.That(
                    totalAfterUpdate,
                    Is.Not.EqualTo(totalBefore),
                    "El total no cambió luego de actualizar la cantidad."
                );
            }
            else
            {
                var qtyAfter = Driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                    .FirstOrDefault(e => e.Displayed);

                Assert.That(qtyAfter, Is.Not.Null, "No se encontró el input de cantidad después del Update.");

                Assert.That(
                    qtyAfter!.GetAttribute("value"),
                    Is.EqualTo("2"),
                    "La cantidad no quedó en 2 luego de actualizar."
                );
            }

            LogPass("Quantity update validated.");

            LogStep("Remove product from cart");

            var removeBtn = Driver.FindElements(By.XPath(
                "//a[contains(@title,'Remove') or contains(@title,'Delete') or contains(@href,'remove') or contains(@href,'delete')] | " +
                "//button[contains(.,'Remove') or contains(.,'Delete')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(removeBtn, Is.Not.Null, "No se encontró botón Remove/Delete.");

            removeBtn!.Click();

            LogStep("Wait for empty cart state");

            var waitEmpty = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));

            waitEmpty.Until(d =>
            {
                var html = d.PageSource.ToLower();

                return html.Contains("your shopping cart is empty")
                    || html.Contains("shopping cart is empty")
                    || d.FindElements(By.XPath("//input[contains(@name,'quantity')]")).Count == 0;
            });

            Assert.That(
                Driver.PageSource.ToLower(),
                Does.Contain("empty").Or.Contain("shopping cart"),
                "No se detectó estado de carrito vacío."
            );

            LogPass("Cart is empty after removing product.");
        }

        [Test]
        [Category("Cart")]
        public void Cart_RemoveProduct_ShouldLeaveCartEmpty()
        {
            LogStep("Search product 'shampoo' from homepage");

            var home = new HomePage(Driver!)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            LogStep("Open first product from search results");

            var product = home.OpenFirstProduct();

            LogStep("Add product to cart");

            var cart = product.AddToCart();

            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se mostró."
            );

            LogPass("Product successfully added to cart.");

            LogStep("Remove product from cart");

            cart.RemoveFirstItem();

            LogStep("Validate cart is empty");

            Assert.That(
                cart.IsCartEmpty(),
                Is.True,
                "El carrito no quedó vacío luego de eliminar el producto."
            );

            LogPass("Cart is empty after removing product.");
        }

        [Test]
        [Category("Navigation")]
        public void Category_Navigation_And_ProductDetails_ShouldSucceed()
        {
            LogStep("Open homepage and navigate to Skincare category");

            var category = new CategoryPage(Driver!)
                .GoToHome(TestConfig.Instance.BaseUrl)
                .OpenCategoryByText("Skincare");

            LogStep("Open first product from category");

            var product = category.OpenFirstProductFromCategory();

            LogStep("Get product name from product details page");

            var productName = product.GetName();

            LogInfo("PRODUCT NAME: " + productName);

            Assert.That(
                productName,
                Is.Not.Empty,
                "No se pudo obtener el nombre del producto."
            );

            LogPass("Product details page loaded successfully.");
        }
    }
}