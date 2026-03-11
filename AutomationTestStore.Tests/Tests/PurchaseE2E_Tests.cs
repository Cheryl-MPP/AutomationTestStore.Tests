using AutomationTestStore.Tests.Config;
using AutomationTestStore.Tests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;


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
        [Category("OldUser")]//Indica que el escenario usa un usuario existente o previamente registrado.
        public void Login_RegisteredUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");

            //El navegador se dirige a la página principal del sitio y
            //esto simula cuando un usuario abre el sitio web antes de iniciar sesión.
            Driver!.Navigate().GoToUrl("https://automationteststore.com/");

            LogStep("Open login page");

            //Busca el enlace con el texto:Login or register, hace click, inicia sesión y se registra
            Driver.FindElement(By.LinkText("Login or register")).Click();

            LogStep("Enter registered username");
            //Acá encuentra el campo de usuario mediante su id:loginFrm_loginname
            //y escribe el nombre del usuario registrado: cheryltest123
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");

            LogStep("Enter password");
            //Localiza el campo de contraseña:loginFrm_password
            //y escribe la contraseña correspondiente.
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");

            LogStep("Click login button");
            //Busca el botón cuyo atributo title es:Login
            //y hace clic para enviar el formulario de inicio de sesión.
            //Esto provoca que el sistema:
            //1- valide las credenciales
            //2- cree una sesión
            //3- redirija al usuario a su cuenta
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();

            LogStep("Validate successful login");

            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se detectó la página de cuenta después del login."
            );

            LogPass("Registered user login completed successfully.");
        }//Con el método anterior validamos que después de hacer login,
         //el sistema debería redirigir a la página de cuenta del usuario
         //el test valida que el título de la página contenga la palabra "account"
         //si el título no contiene esa palabra, el test falla.
         //y esto indicaría que: el login no fue exitoso y
         //que el usuario no fue redirigido correctamente
         // caso contrario, si la validación pasa,
         // el test registra en los logs
         // que el inicio de sesión se
         // completó correctamente.

        [Test]
        [Category("Purchase")]
        [Category("OldUser")]
        public void Purchase_Product_AuthenticatedUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");
            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);
            //se abre el test, se abre la página principal
            LogStep("Open login page");
            Driver.FindElement(By.LinkText("Login or register")).Click();
            //entra al login
            LogStep("Enter registered username");
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");
            //escribe usuario

            LogStep("Enter password");// escribe contraseña y hace click en btn login
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");

            LogStep("Click login button");
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();
            ////acá se prueba que el usuario registrado pueda autenticarse correctamente


            //Buscar y agregar el producto al carrito
            //Esta parte después del login, el test:
            //vuelve a la home
            //busca el producto shampoo
            //abre el primer resultado
            //lo agrega al carrito
            //valida que el carrito sí esté visible
            //Y prueba que un usuario autenticado 
            //puede seleccionar un producto y 
            //llevarlo al carrito.
            LogStep("Wait for successful login");//Después de hacer login
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            wait.Until(d =>
                d.Url.ToLower().Contains("account") ||//espera que la URL contenga account
                d.Title.ToLower().Contains("account"));//y el título también contenga account,

            LogInfo("LOGIN URL: " + Driver.Url);//lo cual confirmaría que el login fue exitoso
            LogInfo("LOGIN TITLE: " + Driver.Title);//y se redirigió a la página de cuenta.  

            //Luego hace un Assert para confirmar que realmente sí entró.
            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");//Si el assert pasa, se registra que el login fue exitoso.


            //Después del login, el test: vuelve a la home
            LogStep("Search for product 'shampoo'");//busca el producto shampoo
            var home = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            LogStep("Open first product");
            var product = home.OpenFirstProduct();//abre el primer resultado

            LogStep("Add product to cart");//lo agrega al carrito
            var cart = product.AddToCart();

            LogStep("Validate cart is visible");//valida que el carrito sí esté visible
            Assert.That(cart.IsCartVisible(), Is.True, "El carrito no se ve");

            LogPass("Cart is visible.");

            LogStep("Proceed to checkout");
            var checkout = cart.ProceedToCheckout();
            //Desde el carrito, el test hace clic
            //en checkout para iniciar el proceso
            //de compra.


            //Aquí el test contempla dos posibles
            //comportamientos del sistema:
            LogStep("Continue checkout flow");

            //Opción A: aparece un formulario
            //intermedio Si aparece, el test lo llena con:
            //checkout.FillGuestFormAndContinue();nombre,
            //apellido, email, teléfono, dirección, ciudad,
            //país, provincia y código postal
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

            //Esto guarda evidencia del punto exacto en el que
            //está el navegador antes de confirmar la orden.
            //Mas que todo por debugging
            LogInfo("AUTH BEFORE CONFIRM URL: " + Driver.Url);
            LogInfo("AUTH BEFORE CONFIRM TITLE: " + Driver.Title);

            LogStep("Confirm order");//Presiona el botón para confirmar la compra.
            var success = confirm.ConfirmOrder();

            LogInfo("AUTH AFTER CONFIRM URL: " + Driver.Url);
            LogInfo("AUTH AFTER CONFIRM TITLE: " + Driver.Title);

            LogStep("Validate success URL");

            //Después de confirmar, el test valida que la URL
            //sea la de éxito, lo cual confirmaría que la
            //orden se completó correctamente.
            Assert.That(
                Driver.Url.ToLowerInvariant(),
                Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito."
            );

            //Si el assert anterior pasa, se registra
            //que se detectó la URL de éxito, lo cual
            //es un indicador clave de que la compra
            //se completó correctamente.
            LogPass("Success URL detected.");//Valida URL

            LogStep("Validate success message");
            //Extrae el texto de la página final
            //y valida que contenga frases de
            //compra exitosa, como: your order,
            //processed, success, thank you, etc.

            var successText = success.GetSuccessText().ToLowerInvariant();

            //A partir de aquí, el sistema debería completar
            //Y finalmente, el test valida que en la página de
            //éxito aparezca un mensaje de confirmación de compra,
            //Porque no solo valida la URL, sino también el
            //contenido visible en pantalla con el assert
            Assert.That(
                successText,
                Does.Contain("your order").Or.Contain("processed").Or.Contain("success"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Order completed successfully.");//orden fue completada correctamente.
        }


        [Test]
        [Category("PurchaseMultiple")]//Clasifica el test como compra de múltiples productos.
        [Category("OldUser")]//Indica que el flujo usa un usuario ya registrado.
        public void Purchase_MultipleProducts_AuthenticatedUser_ShouldSucceed()
        {
            LogStep("Open Automation Test Store homepage");

            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);//Abre la página principal del sitio.

            LogStep("Login with registered user");

            Driver.FindElement(By.LinkText("Login or register")).Click();//entra al login
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");//ingresa usuario
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");//ingresa contraseña
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();//presiona Login


            //valida que el título de la página contenga
            //account, lo que indica que el usuario
            //fue redirigido a su cuenta.
            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");

            //Aquí vuelve a la página principal
            LogStep("Search and add first product 'shampoo'");//busca el producto shampoo

            var home = new HomePage(Driver)
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");

            var product1 = home.OpenFirstProduct();//abre el primer resultado
            var cart = product1.AddToCart();//lo agrega al carrito

            //Confirma que el carrito aparece correctamente
            //después de agregar el primer producto.
            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se ve después del primer producto"
            );

            //Ahora el test repite el proceso con otro producto:

            LogPass("First product added to cart.");

            LogStep("Search and add second product 'cream'");

            var home2 = new HomePage(Driver)//vuelve a la home
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("cream");//busca cream

            var product2 = home2.OpenFirstProduct();//abre el primer resultado
            cart = product2.AddToCart();
            //Esto permite probar un carrito con múltiples productos.


            //Confirma que el carrito sigue visible después de agregar el segundo producto.
            Assert.That(
                cart.IsCartVisible(),//Esto indica que ambos productos fueron agregados correctamente.
                Is.True,
                "El carrito no se ve después del segundo producto"
            );

            LogPass("Second product added to cart.");

            LogStep("Proceed to checkout");

            cart.ProceedToCheckout();//El test inicia el proceso de compra desde el carrito.

            LogInfo("MULTI AUTH CHECKOUT URL: " + Driver.Url);//URL
            LogInfo("MULTI AUTH CHECKOUT TITLE: " + Driver.Title);//Título
            //guarda logs, esto ayuda a depurar si algo falla.

            LogStep("Confirm order");

            var confirm = new CheckoutConfirmPage(Driver);//Presiona el botón para confirmar la compra.

            var success = confirm.ConfirmOrder();//procesa la compra y redirige a la página de éxito.

            LogInfo("MULTI AUTH AFTER CONFIRM URL: " + Driver.Url);//URL final
            LogInfo("MULTI AUTH AFTER CONFIRM TITLE: " + Driver.Title);//título final
            //después de confirmar la compra.

            LogStep("Validate success URL");

            //Valida que la URL contenga algo como:
            //checkout/success o success, lo cual confirmaría que
            //la orden se completó correctamente.
            Assert.That(
                Driver.Url.ToLowerInvariant(),
                Does.Contain("checkout/success").Or.Contain("success"),
                "No se detectó navegación a página de éxito."
            );

            LogStep("Validate success message");

            //Obtiene el texto visible en la página final y
            //valida que incluya frases como:
            //your order
            //processed
            //success
            //Esto confirma que la orden fue procesada.
            var successText = success.GetSuccessText().ToLowerInvariant();

            Assert.That(
                successText,
                Does.Contain("your order")
                .Or.Contain("processed")
                .Or.Contain("success"),
                "No se detectó mensaje de éxito en la página."
            );

            LogPass("Multiple product purchase completed successfully.");
        } //Si todo sale bien, el test registra que la compra de múltiples
          //productos fue exitosa.

        [Test]
        [Category("Cart")]//pertenece al módulo de carrito.
        [Category("OldUser")]//usa un usuario ya registrado.
        public void Cart_UpdateQuantity_RemoveProduct_ValidateTotals_ShouldSucceed()
        {
            LogStep("Login with registered user");

            Driver!.Navigate().GoToUrl(TestConfig.Instance.BaseUrl);//abre el sitio

            Driver.FindElement(By.LinkText("Login or register")).Click();//entra a login
            Driver.FindElement(By.Id("loginFrm_loginname")).SendKeys("cheryltest123");//escribe usuario
            Driver.FindElement(By.Id("loginFrm_password")).SendKeys("Password123");//escribe contraseña
            Driver.FindElement(By.CssSelector("button[title='Login']")).Click();//presiona login


            //Valida que el login fue exitoso
            //confirmando que el título de la
            //página contenga "account",
            //lo que indicaría que se redirigió
            //a la página de cuenta del usuario
            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("account"),
                "No se logró iniciar sesión."
            );

            LogPass("User logged in successfully.");

            LogStep("Search product and add to cart");

            var home = new HomePage(Driver)//vuelve a la página principal
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo"); //busca el producto shampoo

            var product = home.OpenFirstProduct();//abre el primer resultado
            var cart = product.AddToCart();//lo agrega al carrito

            //Valida que el producto sí fue agregado
            //y que el carrito se muestra correctamente
            Assert.That(
                cart.IsCartVisible(),
                Is.True,
                "El carrito no se ve después de agregar producto."
            );

            LogPass("Product added to cart.");

            LogStep("Open Shopping Cart page");

            //El test navega directamente a la URL del carrito.
            Driver.Navigate().GoToUrl($"{TestConfig.Instance.BaseUrl}/index.php?rt=checkout/cart");
            //Esto asegura que el flujo se posicione
            //exactamente en la pantalla donde
            //se quiere trabajar: el carrito.

            //Valida que el título de la página corresponda al carrito.
            Assert.That(
                Driver.Title.ToLower(),
                Does.Contain("shopping cart").Or.Contain("cart"),
                "No abrió Shopping Cart."
            );

            //Leer el total inicial del carrito
            LogStep("Read initial cart total");

            decimal ReadTotal()//Busca en la página el total del carrito,
                               //extrae el texto, limpia símbolos y
                               //trata de convertirlo a número decimal
            {
                //Busca posibles elementos donde esté el total
                var candidates = Driver.FindElements(By.XPath(//Usa XPath para encontrar algo asociado a la palabra total y el precio siguiente.
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::span[contains(@class,'price')][1] | " +
                    "//*[contains(translate(normalize-space(.),'TOTAL','total'),'total')]/following::td[1]"
                ));//Si no encuentra ese elemento, prueba con otros selectores
                   //más genéricos, buscando el último precio
                   //visible en la página como respaldo.

                //Prueba con otro enfoque
                var raw = candidates.FirstOrDefault()?.Text ?? "";//Toma el texto del primer
                                                                  //candidato encontrado, o
                                                                  //una cadena vacía si no
                                                                  //encuentra ninguno.

                if (string.IsNullOrWhiteSpace(raw))
                {
                    var prices = Driver.FindElements(By.CssSelector("span.price"));//Si no encuentra el total con el método
                                                                                   //anterior, busca todos los elementos
                                                                                   //que tengan la clase price y toma el
                                                                                   //último visible como posible total.
                    raw = prices.LastOrDefault()?.Text ?? "";//Limpia el texto
                }//quita $, US, comas, espacios para tratar de dejar solo el número

                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw,//Intenta convertir el texto limpio a decimal usando la cultura invariante
                        System.Globalization.NumberStyles.Any,//Permite cualquier formato numérico común, como decimales, miles, etc
                        System.Globalization.CultureInfo.InvariantCulture,//Usa la cultura estándar para evitar problemas con formatos numéricos específicos de cada región
                        out var val))//Si la conversión es exitosa, devuelve el valor decimal
                    return val;//Si no se pudo convertir, intenta con la cultura de Costa Rica, que es común en esta tienda

                if (decimal.TryParse(raw,
                        System.Globalization.NumberStyles.Any,
                        new System.Globalization.CultureInfo("es-CR"),//Si no funciona, prueba con:es-CR
                        out val))
                    return val;

                Console.WriteLine("No se pudo parsear TOTAL. RAW=" + raw);//Si no puede parsearlo
                return -1m;//devuelve -1 para indicar que no se pudo obtener un total válido
            }//Eso significa: no pude leer el total correctamente

            var totalBefore = ReadTotal();
            LogInfo("TOTAL BEFORE: " + totalBefore);
            //Lee el total antes de modificar la cantidad
            //y lo guarda para compararlo después.


            //Busca el input de cantidad del producto en el carrito.
            LogStep("Update quantity to 2");

            var qtyInput = Driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                .FirstOrDefault(e => e.Displayed && e.Enabled);

            //Luego valida que sí exista ese input, lo limpie
            //y escriba el número 2 para actualizar la cantidad
            //a 2 unidades del producto
            Assert.That(qtyInput, Is.Not.Null, "No se encontró input de cantidad en el carrito.");

            qtyInput!.Clear();//Después borra el valor anterior 
            qtyInput.SendKeys("2");//y luego escribe 2

            LogStep("Click Update button");

            //Buscar y hacer clic en Update
            //Aquí lo que hace es buscar el botón Update
            //en distintas formas posibles:
            //botón
            //input
            //enlace
            var updateBtn = Driver.FindElements(By.XPath(
                "//button[contains(.,'Update') or contains(@title,'Update')] | " +
                "//input[@value='Update' or contains(@title,'Update')] | " +
                "//a[contains(.,'Update')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);
            //lo hace así oorque dependiendo del HTML
            //real del sitio, el botón podría estar
            //implementado distinto


            //Valida que el botón Update exista y esté visible y habilitado
            Assert.That(updateBtn, Is.Not.Null, "No se encontró botón Update en el carrito.");

            updateBtn!.Click();//Si el botón existe, hace clic para actualizar la cantidad.

            LogStep("Wait for quantity update");

            var waitQty = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));


            //Después de hacer clic en Update,
            //el test espera hasta que el
            //valor del input de cantidad
            //se actualice a 2.
            waitQty.Until(d =>
            {
                try
                {
                    var input = d.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                        .FirstOrDefault(e => e.Displayed);

                    if (input == null) return false;

                    return input.GetAttribute("value") == "2";//Espera hasta que el input de cantidad realmente tenga valor "2".
                }
                catch (StaleElementReferenceException)//Porque al actualizarse el carrito, Selenium puede perder la referencia al elemento viejo.Entonces el test vuelve a intentar.
                {
                    return false;
                }
            });

            LogStep("Read updated total");//Después de hacer clic en Update, el cambio puede tardar unos segundos.

            decimal TryReadCartTotal()//Intenta leer otra vez el total del carrito, usando selectores CSS.
            {
                var totalEl = Driver.FindElements(By.CssSelector(//busca elementos como:
                    ".total .price, #totals_table .price, .cart_total .price, .cart-info .price, span.price"//estos selectores
                )).LastOrDefault();

                var raw = totalEl?.Text ?? "";

                raw = raw.Replace("$", "").Replace("US", "").Replace(",", "").Trim();

                if (decimal.TryParse(raw,////Luego limpia el texto y trata de convertirlo a decimal.
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var val))
                    return val;

                return -1m;//Si no puede, devuelve -1.
            }

            var totalAfterUpdate = TryReadCartTotal();//Luego el test compara dos escenarios:

            //Escenario A: sí se pudieron leer ambos totales
            LogInfo("TOTAL AFTER UPDATE: " + totalAfterUpdate);

            if (totalBefore >= 0 && totalAfterUpdate >= 0)//Valida que el total nuevo sea distinto al anterior:
            {
                //significa que si la cantidad subió de 1 a 2, el total debería cambiar
                Assert.That(
                    totalAfterUpdate,
                    Is.Not.EqualTo(totalBefore),
                    "El total no cambió luego de actualizar la cantidad."
                );
            }
            else
            {//Escenario B: no se pudieron leer bien los totales
                var qtyAfter = Driver.FindElements(By.XPath("//input[contains(@name,'quantity')]"))
                    .FirstOrDefault(e => e.Displayed);
                Assert.That(qtyAfter, Is.Not.Null, "No se encontró el input de cantidad después del Update.");

                Assert.That(
                    qtyAfter!.GetAttribute("value"),
                    Is.EqualTo("2"),////Y valida que el input realmente tenga valor "2".
                    "La cantidad no quedó en 2 luego de actualizar."
                );//significa que aunque no se pudo leer el total,
                  //al menos sí se actualizó la cantidad a 2,
                  //lo cual es un indicador de que el proceso de
                  //actualización sí funcionó aunque no se pueda
                  //validar el total.
            }

            LogPass("Quantity update validated.");

            LogStep("Remove product from cart");

            //Busca un botón de eliminar usando varias posibilidades:
            var removeBtn = Driver.FindElements(By.XPath(
                "//a[contains(@title,'Remove') or contains(@title,'Delete') or contains(@href,'remove') or contains(@href,'delete')] | " +
                "//button[contains(.,'Remove') or contains(.,'Delete')]"
            )).FirstOrDefault(e => e.Displayed && e.Enabled);

            Assert.That(removeBtn, Is.Not.Null, "No se encontró botón Remove/Delete.");

            removeBtn!.Click();//Luego hace clic.
            //es muy grande el método porque contempla muchas posibilidades
            //de cómo el sitio podría estar implementando el botón de eliminar
            //producto del carrito, y también contempla distintos escenarios
            //para validar la actualización de cantidad y el cambio en el total

            LogStep("Wait for empty cart state");

            var waitEmpty = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));

            waitEmpty.Until(d =>
            {
                var html = d.PageSource.ToLower();//Espera hasta que se cumpla alguna de estas condiciones:

                return html.Contains("your shopping cart is empty")//aparece texto indicando que el carrito está vacío
                    || d.FindElements(By.XPath("//input[contains(@name,'quantity')]")).Count == 0;//ya no existen inputs de cantidad
            });//Esto indica Porque no depende de una sola señal;
               //usa varias formas de confirmar que el carrito quedó vacío.


            //Valida que la página muestre evidencia de que el carrito está vacío
            Assert.That(
                Driver.PageSource.ToLower(),
                Does.Contain("empty").Or.Contain("shopping cart"),
                "No se detectó estado de carrito vacío."
            );

            //Si todo sale bien, el test registra que el producto
            //fue removido correctamente y el carrito quedó vacío.
            LogPass("Cart is empty after removing product.");
        }

        [Test]
        [Category("Cart")]//Clasifica el test dentro del módulo de carrito de compras.
        public void Cart_RemoveProduct_ShouldLeaveCartEmpty()
        {
            LogStep("Search product 'shampoo' from homepage");

            var home = new HomePage(Driver!)//abre la página principal
                .GoTo(TestConfig.Instance.BaseUrl)
                .Search("shampoo");//busca el producto shampoo

            LogStep("Open first product from search results");

            var product = home.OpenFirstProduct();
            //Abre el primer producto que aparece en los resultados de búsqueda.
            LogStep("Add product to cart");

            var cart = product.AddToCart();
            //Presiona el botón Add to Cart

            //Valida que el carrito sea visible después de agregar el producto.
            Assert.That(
                cart.IsCartVisible(),
                Is.True,//Esto confirma que el producto se añadió correctamente.
                "El carrito no se mostró."
            );

            LogPass("Product successfully added to cart.");

            LogStep("Remove product from cart");

            cart.RemoveFirstItem();
            //Ese método hace clic en el botón Remove / Delete del carrito.

            LogStep("Validate cart is empty");

            //Valida que el carrito esté vacío después de eliminar el producto.
            Assert.That(
                cart.IsCartEmpty(),
                Is.True,//Lo confirma
                "El carrito no quedó vacío luego de eliminar el producto."
            );

            LogPass("Cart is empty after removing product.");
        }//Si la validación pasa, el test registra que el carrito quedó vacío correctamente.

        [Test]
        [Category("Navigation")]//Clasifica el test dentro de las pruebas de navegación del sitio
        public void Category_Navigation_And_ProductDetails_ShouldSucceed()
        {
            LogStep("Open homepage and navigate to Skincare category");

            var category = new CategoryPage(Driver!)
                .GoToHome(TestConfig.Instance.BaseUrl)//abre la página principal
                .OpenCategoryByText("Skincare");//navega a la categoría Skincare

            LogStep("Open first product from category");

            //abre el primer producto que aparece dentro de la categoría Skincare
            var product = category.OpenFirstProductFromCategory();

            LogStep("Get product name from product details page");
            //Una vez abierta la página de detalles del producto,
            //el test obtiene el nombre del producto
            var productName = product.GetName();//Ese nombre se extrae del DOM de la página.
            //puede ser Seaweed Conditioner o Aloe Vera Cream
            LogInfo("PRODUCT NAME: " + productName);//Guarda el nombre en los logs del test.
            //ayuda con el debugging y revisar que producto se abrió realmente


            //valida que el nombre del producto no esté vacío
            Assert.That(
                productName,
                Is.Not.Empty,
                "No se pudo obtener el nombre del producto."
            );

            LogPass("Product details page loaded successfully.");
        }//Si todo funciona, el test registra que la página de
         //detalles del producto se cargó correctamente
    }
}