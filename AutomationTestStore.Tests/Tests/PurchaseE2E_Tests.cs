using AutomationTestStore.Tests.Config;
using AutomationTestStore.Tests.Pages;
using AutomationTestStore.Tests.Reports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Buffers.Text;
using System.Security.Policy;


namespace AutomationTestStore.Tests.Tests
{
    [NonParallelizable]
    // dado que algunos tests hacen limpieza
    // de procesos, es mejor no correr en paralelo
    // para evitar conflictos. Si se quiere paralelizar,
    // habría que aislar aún más el driver y la limpieza.
    [TestFixture]
    // esta clase es para tests de compra E2E, pero
    // también incluye algunos tests relacionados a
    // navegación, registro y carrito para aprovechar
    // el setup común. Se pueden mover a otras clases
    // si se quiere.
    public class PurchaseE2E_Tests : BaseTest
    {
        //Este test automatiza el proceso completo
        //de compra de un producto como usuario
        //invitado en la tienda Automation Test
        //Store y valida que la orden se complete
        //correctamente.

        [Test]
        [Category("Purchase")]
        public void Purchase_Product_GuestCheckout_ShouldSucceed()
        {
            //Un usuario puede buscar un producto
            LogStep("Search product 'shampoo' from homepage");

            var home = new HomePage(Driver!)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            LogInfo("URL: " + Driver!.Url);//URL actual después de la búsqueda
            LogInfo("TITLE: " + Driver!.Title);//título de la página

            LogStep("Open first product from search results");//abre el primer producto de la lista

            var product = home.OpenFirstProduct();//Simula cuando un usuario hace click en el primer resultado.
            //Agregarlo al carrito
            LogStep("Add product to cart");//Agrega el producto al carrito desde la página de detalles del producto.

            var cart = product.AddToCart();//Valida que el carrito se muestre correctamente después de agregar el producto.

            LogStep("Validate cart is visible");

            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");//Valida que el carrito realmente esté visible.

            LogPass("Cart displayed successfully.");
            //Proceder al checkout
            LogStep("Proceed to checkout");

            var checkout = cart.ProceedToCheckout();//Simula cuando el usuario hace click en: Checkout y el
                                                    //sistema abre la página de checkout.
            //Completar el formulario como invitado
            LogStep("Fill guest checkout form");

            var confirm = checkout.FillGuestFormAndContinue();//Aquí llenamos el form y luego press continue 
            //lo cual permite que comprar como invitado/guest

            LogInfo("BEFORE CONFIRM URL: " + Driver!.Url);//confirma el navegador 
            LogInfo("BEFORE CONFIRM TITLE: " + Driver!.Title);//si realmente llegó al paso de confirmación del checkout.
            //Confirma la orden
            LogStep("Confirm order");

            var success = confirm.ConfirmOrder();//procesa la compra y redirige a la pagina de success

            LogInfo("AFTER CONFIRM URL: " + Driver!.Url);
            LogInfo("AFTER CONFIRM TITLE: " + Driver!.Title);

            LogStep("Validate checkout confirmation step finished");
            //aquí este método valida que no se haya quedado atascado
            //en el paso de confirmación del checkout, lo cual
            //indicaría que la orden no se completó correctamente.
            Assert.That(
                Driver!.Url,
                Does.Not.Contain("guest_step_3"),//significa que la orden no se completó.
                "Se quedó en Checkout Confirmation; no se completó la orden."
            );
            //Llegar a la página de éxito
            LogStep("Validate success URL");

            //en todo este método se validan varias URLs posibles de éxito, porque dependiendo del
            //flujo puede variar la URL final. Lo importante es validar que se detecte alguna URL
            //de éxito, para confirmar porque puede ser success, checkout/success guest_step_4o guest_step_4/5
            //dependiendo del flujo.
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

            var successText = success.GetSuccessText().ToLowerInvariant();// valida que la web muestre Thank you!
            // Your order has been successfully processed!


            //Esto confirma que en la página aparece un mensaje de confirmación de compra.
            Assert.That(
                successText,
                Does.Contain("success")
                .Or.Contain("your order")
                .Or.Contain("checkout success")
                .Or.Contain("thank you"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Guest checkout completed successfully.");
        }//Si todo pasó: El test registra en el reporte que el checkout fue exitoso.

        //Falso negativo : sabemos que no hay productos con ese nombre,
        //pero validamos que sí existan resultados (aunque no existan)
        [Test]
        [Category("Negative-Demo")]
        public void Search_ProductNotFound_ShouldFail()
        { 
            LogStep("Open Automation Test Store homepage");

            var home = new HomePage(Driver!)//crea el pom de la web y
                .GoTo(TestConfig.Instance.BaseUrl);//Abre la página principal de la tienda.

            LogStep("Search for a non-existing product");//Realizamos la búsqueda lamborghini12345 producto que no existe
            //se  muestran 0 resultados, pero el test espera que sí haya resultados,
            //lo cual fuerza un falso negativo para validar que el reporte muestre
            //el error y capture la pantalla correctamente.
            home = home.Search("lamborghini12345");

            LogInfo("NEGATIVE URL: " + Driver!.Url);//URL después de la búsqueda
            LogInfo("NEGATIVE TITLE: " + Driver!.Title);//título de la página después de la búsqueda

            LogStep("Validate search results");//¿La búsqueda devolvió productos?

            var hasResults = home.HasSearchResults();//Valida si la búsqueda mostró resultados.
                                                     //En este caso, como el producto no existe,
                                                     //debería ser false, pero el test espera que
                                                     //sea true, lo cual fuerza un error para
                                                     //validar el reporte.

            LogStep("Force failure to validate reporting and screenshot");//El test dice:¡Esperamos que haya resultados!
            Assert.That(hasResults, Is.True,//pero...
                "Escenario negativo: no se encontraron productos para la búsqueda.");//nada de resultados false y el test falla
        }

        [Test]//Indica que NUnit debe ejecutar este método como un caso de prueba.
        [Category("Register")]//Clasifica el test como parte del módulo de registro.
        [Category("NewUser")]//Indica que es específicamente un caso de usuario nuevo.
        public void Register_NewUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");//Abre la página principal del sitio web.
            Driver!.Navigate().GoToUrl("https://automationteststore.com/");//Simula al usuario entrando manualmente
                                                                           //al sitio para iniciar el proceso.

            LogStep("Open login or register page");
            Driver.FindElement(By.LinkText("Login or register")).Click();//busca el enlace de "Login or register" y
                                                                         //hace click para abrir la página de
                                                                         //login/registro.

            LogStep("Click continue to open registration form");
            Driver.FindElement(By.CssSelector("button[title='Continue']")).Click();//Presiona el botón Continue
                                                                                   //para abrir el formulario de
                                                                                   //registro de nuevo usuario.
                                                                                   //O sea, no entra por login,
                                                                                   //sino por la opción de crear
                                                                                   //cuenta.

            LogStep("Wait for registration form to be visible");
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));//Aquí se crea una espera explícita
                                                                           //de hasta 20 segundos.
            wait.Until(d => d.FindElements(By.Id("AccountFrm_firstname")).Count > 0);//Luego el test espera hasta
                                                                                     //que aparezca el campo:
                                                                                     //AccountFrm_firstname
                                                                                     //Eso significa que el formulario
                                                                                     //ya cargó y está listo para usarse.
            
            //Aquí Genera una cadena aleatoria de 6 caracteres usando Guid.
            LogStep("Generate random user data to avoid duplicates");
            string random = Guid.NewGuid().ToString("N").Substring(0, 6);//guid ejm-> a4f9c2
                                                                         //Esto se hace para evitar conflictos con usuarios
                                                                         //ya existentes en la base de datos de la tienda,
                                                                         //ya que el test crea un nuevo usuario
                                                                         //en cada ejecución.

            LogStep("Fill registration form");//completamos el form
            Driver.FindElement(By.Id("AccountFrm_firstname")).SendKeys("Test");
            Driver.FindElement(By.Id("AccountFrm_lastname")).SendKeys("User");
            Driver.FindElement(By.Id("AccountFrm_email")).SendKeys($"test{random}@mail.com");//correo único usando la cadena aleatoria
            Driver.FindElement(By.Id("AccountFrm_telephone")).SendKeys("88888888");
            Driver.FindElement(By.Id("AccountFrm_address_1")).SendKeys("Test Address");
            Driver.FindElement(By.Id("AccountFrm_city")).SendKeys("San Jose");

            //acá esperamos que el dropdown de país sea clickeable
            LogStep("Select country");
            var countryElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("AccountFrm_country_id")));
            var countrySelect = new SelectElement(countryElement);//Crea un objeto SelectElement
            countrySelect.SelectByText("Costa Rica");//Selecciona "Costa Rica" del dropdown de país.
            // usa SelectElement porque es un combo box /select HTML  u no un input normal

            //Cuando se selecciona un país, el dropdown de provincia
            //o estado suele recargarse dinámicamente.
            LogStep("Wait for province/state dropdown to refresh");
            wait.Until(d =>
            {
                try
                {
                    var zone = d.FindElement(By.Id("AccountFrm_zone_id"));
                    var options = zone.FindElements(By.TagName("option"));
                    return zone.Displayed && zone.Enabled && options.Count > 1;
                }
                catch (StaleElementReferenceException) //Eso pasa mucho cuando la página refresca elementos dinámicamente.
                {
                    return false;
                }
                catch (NoSuchElementException)//al igual con esta 
                {
                    return false;
                }
            });//Este bloque espera hasta que:el dropdown
               //exista,esté visible,esté habilitado, y 
               //tenga más de una opción disponible.
               //Es importante porque si el test intenta
               //seleccionar provincia demasiado rápido,
               //puede fallar porque el combo todavía
               //no se actualiza.

            LogStep("Select province/state");//Selecciona una opción del dropdown de provincia/estado.
            var zoneElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("AccountFrm_zone_id")));
            var zoneSelect = new SelectElement(zoneElement);
            zoneSelect.SelectByIndex(1);//toma la segunda opción
                                        //disponible.No selecciona
                                        //por nombre, sino por posición
                                        //en la lista.

            //acá se completa el resto del formulario 
            //randomiza el username también para evitar
            //conflictos con usuarios existentes user{random} ejm-> usera4f9c2
            //Así se evita duplicidad de username.
            Driver.FindElement(By.Id("AccountFrm_postcode")).SendKeys("1000");
            Driver.FindElement(By.Id("AccountFrm_loginname")).SendKeys($"user{random}");
            Driver.FindElement(By.Id("AccountFrm_password")).SendKeys("Password123");
            Driver.FindElement(By.Id("AccountFrm_confirm")).SendKeys("Password123");

            LogStep("Accept terms and conditions");
            Driver.FindElement(By.Id("AccountFrm_agree")).Click();//Marca el checkbox de aceptación
                                                                  //de términos.Es un *

            LogStep("Submit registration");
            Driver.FindElement(By.CssSelector("button[title='Continue']")).Click();//Presiona el botón Continue para
                                                                                   //enviar el formulario ya lleno.

            //este bloque del test espera hasta que se cumpla
            //una de estas dos condiciones:
            //que la URL contenga account
            //o que el título de la página contenga account
            LogStep("Wait for successful navigation to account page");
            wait.Until(d =>
                d.Url.ToLower().Contains("account") ||
                d.Title.ToLower().Contains("account"));
            //Eso permite validar la navegación sin depender
            //de una sola condición. hace el test más flexible
            //y robusto ante cambios menores en la página.


            //guarda los logs
            LogInfo("FINAL URL: " + Driver.Url);//la URL final
            LogInfo("FINAL TITLE: " + Driver.Title);//el título final
            //Esto ayuda mucho si algo falla y necesitas revisar
            //en qué página quedó.


            //Validar que sí llegó a la cuenta
            LogStep("Validate user was redirected to account page");
            Assert.That(
                Driver.Url.ToLower(),
                Does.Contain("account"),//Comprueba que la URL final contenga la palabra:account
                "No se navegó a la página de cuenta después del registro."
            );

            LogPass("User registration completed successfully.");
        } //Si todo salió bien, se registra que el registro del usuario fue exitoso.

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