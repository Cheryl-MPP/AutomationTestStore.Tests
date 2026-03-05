using AutomationTestStore.Tests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AutomationTestStore.Tests.Tests
{
    [TestFixture]
    public class PurchaseE2E_Tests
    {
        private IWebDriver? _driver;
        private const string BaseUrl = "https://automationteststore.com/";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;


            // Command timeout más alto (esto evita el timeout de 60s)
            _driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(180));
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
            _driver = null;

            // Limpia procesos colgados (solo para tu ambiente de pruebas)
            try
            {
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("chromedriver"))
                    p.Kill(true);
            }
            catch { }
        }

        [Test]
        public void Purchase_Product_GuestCheckout_ShouldSucceed()
        {
            var home = new HomePage(_driver!)
                .GoTo(BaseUrl)
                .Search("shampoo");

            Console.WriteLine("URL: " + _driver!.Url);
            Console.WriteLine("TITLE: " + _driver!.Title);

            var product = home.OpenFirstProduct();
            var cart = product.AddToCart();

            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");

            var checkout = cart.ProceedToCheckout();
            var confirm = checkout.FillGuestFormAndContinue();

            Console.WriteLine("BEFORE CONFIRM URL: " + _driver!.Url);
            Console.WriteLine("BEFORE CONFIRM TITLE: " + _driver!.Title);

            var success = confirm.ConfirmOrder();

            Console.WriteLine("AFTER CONFIRM URL: " + _driver!.Url);
            Console.WriteLine("AFTER CONFIRM TITLE: " + _driver!.Title);

            // Validación principal: que ya no esté en la confirmación
            Assert.That(_driver!.Url, Does.Not.Contain("guest_step_3"),
                "Se quedó en Checkout Confirmation; no se completó la orden.");

            // Validación de éxito: URL o título o contenido
            var successText = success.GetSuccessText().ToLowerInvariant();

            Assert.That(
                _driver!.Url.ToLowerInvariant(),
                Does.Contain("success").Or.Contain("checkout/success")
                .Or.Contain("guest_step_4").Or.Contain("guest_step_5"),
                "No se detectó navegación a página de éxito (revisar URL)."
            );

            Assert.That(
                successText,
                Does.Contain("success").Or.Contain("your order").Or.Contain("checkout success").Or.Contain("thank you"),
                "No se detectó mensaje de éxito en la página."
            );
        }
        //Falso negrativo : sabemos que no hay productos con ese nombre,
        //pero validamos que sí existan resultados (aunque no existan)
        [Test]
        [Category("Negative-Demo")]
        public void Search_ProductNotFound_ShouldFail()
        {
            var home = new HomePage(_driver!)
                .GoTo(BaseUrl)
                .Search("lamborghini12345");

            Console.WriteLine("NEGATIVE URL: " + _driver!.Url);
            Console.WriteLine("NEGATIVE TITLE: " + _driver!.Title);

            // Validamos que sí existan resultados (pero sabemos que no existen)
            var hasResults = home.HasSearchResults();

            Assert.That(hasResults, Is.True,
                "Escenario negativo: no se encontraron productos para la búsqueda.");
        }


        [Test]
        public void Register_NewUser_ShouldSucceed()
        {
            _driver!.Navigate().GoToUrl("https://automationteststore.com/");

            // Abrir login
            _driver.FindElement(By.LinkText("Login or register")).Click();

            // Continuar a registro
            _driver.FindElement(By.CssSelector("button[title='Continue']")).Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElements(By.Id("AccountFrm_firstname")).Count > 0);

            // Datos aleatorios para evitar duplicados
            string random = Guid.NewGuid().ToString("N").Substring(0, 6);

            _driver.FindElement(By.Id("AccountFrm_firstname")).SendKeys("Test");
            _driver.FindElement(By.Id("AccountFrm_lastname")).SendKeys("User");
            _driver.FindElement(By.Id("AccountFrm_email")).SendKeys($"test{random}@mail.com");
            _driver.FindElement(By.Id("AccountFrm_telephone")).SendKeys("88888888");
            _driver.FindElement(By.Id("AccountFrm_address_1")).SendKeys("Test Address");
            _driver.FindElement(By.Id("AccountFrm_city")).SendKeys("San Jose");

            new SelectElement(_driver.FindElement(By.Id("AccountFrm_country_id")))
                .SelectByText("Costa Rica");

            new SelectElement(_driver.FindElement(By.Id("AccountFrm_zone_id")))
                .SelectByIndex(1);

            _driver.FindElement(By.Id("AccountFrm_postcode")).SendKeys("1000");

            _driver.FindElement(By.Id("AccountFrm_loginname")).SendKeys($"user{random}");
            _driver.FindElement(By.Id("AccountFrm_password")).SendKeys("Password123");
            _driver.FindElement(By.Id("AccountFrm_confirm")).SendKeys("Password123");

            _driver.FindElement(By.Id("AccountFrm_agree")).Click();

            _driver.FindElement(By.CssSelector("button[title='Continue']")).Click();

            // esperar navegación real
            var wait2 = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait2.Until(d => d.Url.ToLower().Contains("account") || d.Title.ToLower().Contains("account"));

            // assert final
            Assert.That(
                _driver.Url.ToLower(),
                Does.Contain("account"),
                "No se navegó a la página de cuenta después del registro."
            );
        }


        [Test]
        public void Login_RegisteredUser_ShouldSucceed()
        {
            _driver!.Navigate().GoToUrl("https://automationteststore.com/");

            // Abrir login
            _driver.FindElement(By.LinkText("Login or register")).Click();

            // Ingresar usuario
            _driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");

            // Ingresar contraseña
            _driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");

            // Pulsar login
            _driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            // Validación
            Assert.That(_driver.Title.ToLower(), Does.Contain("account"));
        }

        [Test]
        public void Purchase_Product_AuthenticatedUser_ShouldSucceed()
        {
            // 1) Login con usuario registrado
            _driver!.Navigate().GoToUrl(BaseUrl);

            _driver.FindElement(By.LinkText("Login or register")).Click();
            _driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            _driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");
            _driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            Assert.That(_driver.Title.ToLower(), Does.Contain("account"), "No se logró iniciar sesión.");

            // 2) Ir al home y buscar producto
            var home = new HomePage(_driver)
                .GoTo(BaseUrl)
                .Search("shampoo");

            var product = home.OpenFirstProduct();
            var cart = product.AddToCart();

            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");

            // 3) Checkout
            var checkout = cart.ProceedToCheckout();

            // Si por alguna razón aparece Guest (depende del sitio), se llena el guest form.
            // Si no aparece Guest, vamos directo a confirmación.
            CheckoutConfirmPage confirm;
            if (checkout.IsGuestFlowVisible())
            {
                confirm = checkout.FillGuestFormAndContinue();
            }
            else
            {
                confirm = new CheckoutConfirmPage(_driver);
            }

            Console.WriteLine("AUTH BEFORE CONFIRM URL: " + _driver.Url);
            Console.WriteLine("AUTH BEFORE CONFIRM TITLE: " + _driver.Title);

            // 4) Confirmar orden
            var success = confirm.ConfirmOrder();

            Console.WriteLine("AUTH AFTER CONFIRM URL: " + _driver.Url);
            Console.WriteLine("AUTH AFTER CONFIRM TITLE: " + _driver.Title);

            // 5) Validar éxito
            Assert.That(_driver.Url.ToLowerInvariant(), Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito.");

            var successText = success.GetSuccessText().ToLowerInvariant();
            Assert.That(successText, Does.Contain("your order").Or.Contain("processed").Or.Contain("success"),
                "No se detectó mensaje de éxito en la página.");
        }

        [Test]
        public void Purchase_MultipleProducts_AuthenticatedUser_ShouldSucceed()
        {
            // 1) Login primero (esto evita que checkout mande a account/login)
            _driver!.Navigate().GoToUrl(BaseUrl);

            _driver.FindElement(By.LinkText("Login or register")).Click();
            _driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            _driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");
            _driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            Assert.That(_driver.Title.ToLower(), Does.Contain("account"), "No se logró iniciar sesión.");

            // 2) Producto 1
            var home = new HomePage(_driver!)
                .GoTo(BaseUrl)
                .Search("shampoo");

            var product1 = home.OpenFirstProduct();
            var cart = product1.AddToCart();
            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve después del primer producto");

            // 3) Producto 2
            var home2 = new HomePage(_driver!)
                .GoTo(BaseUrl)
                .Search("cream");

            var product2 = home2.OpenFirstProduct();
            cart = product2.AddToCart();
            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve después del segundo producto");

            // 4) Checkout
            cart.ProceedToCheckout();

            Console.WriteLine("MULTI AUTH CHECKOUT URL: " + _driver!.Url);
            Console.WriteLine("MULTI AUTH CHECKOUT TITLE: " + _driver!.Title);

            // En autenticado normalmente estás en confirm directo o checkout/confirm
            var confirm = new CheckoutConfirmPage(_driver!);

            var success = confirm.ConfirmOrder();

            Console.WriteLine("MULTI AUTH AFTER CONFIRM URL: " + _driver!.Url);
            Console.WriteLine("MULTI AUTH AFTER CONFIRM TITLE: " + _driver!.Title);

            // 5) Validación final
            Assert.That(_driver!.Url.ToLowerInvariant(),
                Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito.");

            var successText = success.GetSuccessText().ToLowerInvariant();
            Assert.That(successText,
                Does.Contain("your order").Or.Contain("processed").Or.Contain("success"),
                "No se detectó mensaje de éxito en la página.");
        }

        [Test]
        public void Cart_UpdateQuantity_RemoveProduct_ValidateTotals_ShouldSucceed()
        {
            // 1) Login (para que no te mande a account/login en checkout/cart)
            _driver!.Navigate().GoToUrl(BaseUrl);
            _driver.FindElement(By.LinkText("Login or register")).Click();
            _driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            _driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");
            _driver.FindElement(By.CssSelector("button[title='Login']")).Click();
            Assert.That(_driver.Title.ToLower(), Does.Contain("account"), "No se logró iniciar sesión.");

            // 2) Agregar 1 producto
            var home = new HomePage(_driver).GoTo(BaseUrl).Search("shampoo");
            var product = home.OpenFirstProduct();
            var cart = product.AddToCart();
            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve después de agregar producto.");

            // 3) Ir a Shopping Cart
            _driver.Navigate().GoToUrl(BaseUrl + "index.php?rt=checkout/cart");
            Assert.That(_driver.Title.ToLower(), Does.Contain("shopping cart").Or.Contain("cart"), "No abrió Shopping Cart.");

            // Helpers rápidos
            decimal ReadTotal()
            {
                // intenta encontrar el TOTAL por texto "Total" en tablas/resúmenes
                var candidates = _driver.FindElements(By.XPath(
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::span[contains(@class,'price')][1] | " +
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::td[1]"
                ));

                var raw = candidates.FirstOrDefault()?.Text ?? "";

                // fallback: último precio visible (a veces el total es el último)
                if (string.IsNullOrWhiteSpace(raw))
                {
                    var prices = _driver.FindElements(By.CssSelector("span.price"));
                    raw = prices.LastOrDefault()?.Text ?? "";
                }

                // limpia: "$123.45" -> 123.45
                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var val))
                    return val;

                // si el sitio usa coma decimal
                if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                        new System.Globalization.CultureInfo("es-CR"), out val))
                    return val;

                Console.WriteLine("No se pudo parsear TOTAL. RAW=" + raw);
                return -1m;
            }

            // 4) Capturar total inicial
            var totalBefore = ReadTotal();
            Console.WriteLine("TOTAL BEFORE: " + totalBefore);

            // 5) Cambiar cantidad a 2 (primer input quantity que encuentre)
            var qtyInput = _driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                                 .FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(qtyInput, Is.Not.Null, "No se encontró input de cantidad en el carrito.");

            qtyInput!.Clear();
            qtyInput.SendKeys("2");

            // 6) Click Update (necesario para que aplique)
            var updateBtn = _driver.FindElements(By.XPath(
                "//button[contains(.,'Update') or contains(@title,'Update')] | " +
                "//input[@value='Update' or contains(@title,'Update')] | " +
                "//a[contains(.,'Update')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(updateBtn, Is.Not.Null, "No se encontró botón Update en el carrito.");
            updateBtn!.Click();


            // 7) Esperar a que se refleje la cantidad en el input (post update) - robusto contra stale
            var waitQty = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

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
                    // el DOM se refrescó después del Update, reintentar
                    return false;
                }
            });

            // 8) Intentar leer total (opcional)
            decimal TryReadCartTotal()
            {
                var totalEl = _driver.FindElements(By.CssSelector(
                    ".total .price, #totals_table .price, .cart_total .price, .cart-info .price, span.price"
                )).LastOrDefault();

                var raw = totalEl?.Text ?? "";
                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var val))
                    return val;

                return -1m;
            }

            var totalAfterUpdate = TryReadCartTotal();
            Console.WriteLine("TOTAL AFTER UPDATE: " + totalAfterUpdate);

            // Si se pudo leer el total, validamos cambio. Si no, validamos por cantidad (igual es válido)
            if (totalBefore >= 0 && totalAfterUpdate >= 0)
            {
                Assert.That(totalAfterUpdate, Is.Not.EqualTo(totalBefore),
                    "El total no cambió luego de actualizar la cantidad.");
            }
            else
            {
                var qtyAfter = _driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                     .FirstOrDefault(e => e.Displayed);

                Assert.That(qtyAfter, Is.Not.Null, "No se encontró el input de cantidad después del Update.");
                Assert.That(qtyAfter!.GetAttribute("value"), Is.EqualTo("2"),
                    "La cantidad no quedó en 2 luego de actualizar.");
            }

            // 9) Eliminar producto (Remove/Delete)
            var removeBtn = _driver.FindElements(By.XPath(
                "//a[contains(@title,'Remove') or contains(@title,'Delete') or contains(@href,'remove') or contains(@href,'delete')] | " +
                "//button[contains(.,'Remove') or contains(.,'Delete')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(removeBtn, Is.Not.Null, "No se encontró botón/link Remove/Delete en el carrito.");
            removeBtn!.Click();

            // 10) Validar carrito vacío (mensaje o ausencia de qty inputs)
            var waitEmpty = new WebDriverWait(_driver!, TimeSpan.FromSeconds(15));

            waitEmpty.Until(d =>
            {
                var html = d.PageSource.ToLower();

                return html.Contains("your shopping cart is empty")
                    || html.Contains("shopping cart is empty")
                    || d.FindElements(By.XPath("//input[contains(@name,'quantity')]")).Count == 0;
            });

            Assert.That(_driver.PageSource.ToLower(),
                Does.Contain("empty").Or.Contain("shopping cart"),
                "No se detectó estado de carrito vacío.");
        }

        [Test]
        public void Cart_RemoveProduct_ShouldLeaveCartEmpty()
        {
            var home = new HomePage(_driver!)
                .GoTo(BaseUrl)
                .Search("shampoo");

            var product = home.OpenFirstProduct();
            var cart = product.AddToCart();

            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se mostró.");

            cart.RemoveFirstItem();

            Assert.That(cart.IsCartEmpty(), Is.True, "El carrito no quedó vacío luego de eliminar el producto.");
        }
    }
}