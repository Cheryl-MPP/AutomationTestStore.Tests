using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;


namespace AutomationTestStore.Tests.Pages
{
    public class CartPage
    {
        private readonly IWebDriver driver;//Es el navegador controlado por Selenium.
        private readonly WebDriverWait wait;//Es un objeto para hacer esperas explícitas.

        private readonly By CartTable = By.CssSelector(".cart-info");//Localiza la tabla donde aparecen los productos del carrito.
        private readonly By CheckoutButton = By.Id("cart_checkout1");//Localiza el botón Proceed to Checkout.

        private By RemoveButtons => By.CssSelector(//Busca cualquier botón que pueda eliminar productos del carrito.
            "a[title*='Remove'], a[title*='remove'], " +
            "a[href*='remove'], a[href*='delete'], " +
            "button[title*='Remove'], button[title*='remove'], " +
            "input[value*='Remove'], input[value*='remove'], " +
            "a.btn-remove, a.remove, " +
            "a i.fa-trash, a i.icon-trash"
);

        //Busca los campos donde aparece la cantidad de productos
        //Se usa para detectar si el carrito tiene productos.
        private By QuantityInputs => By.CssSelector("input[name*='quantity']");


        //este constructor Inicializa la página, recibe el navegador y
        //crea una espera explícita de 10 segundos
        public CartPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public bool IsCartVisible()//Verifica si el carrito es visible.
        {
            try
            {
                wait.Until(d => d.FindElement(CartTable).Displayed);//Espera hasta que la tabla del carrito aparezca.
                return true;//Si se encuentra y es visible, devuelve true.
            }
            catch
            {
                return false;//Si no se encuentra o no es visible, devuelve false.
            }
        }

        public CartPage RemoveFirstItem()
        {

            //Crea una espera específica para esta acción.
            var waitLocal = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // 1) intentar CSS primero
            IWebElement? remove = null;

            try
            {
                waitLocal.Until(d => d.FindElements(RemoveButtons).Any(e => e.Displayed && e.Enabled));//Espera hasta que haya un botón remove visible y habilitado.
                remove = driver.FindElements(RemoveButtons).FirstOrDefault(e => e.Displayed && e.Enabled);//Luego obtiene el primero:
            }
            catch { /* ignore */ }

            // 2) fallback XPath: buscar cualquier cosa que diga remove/delete o tenga rt=checkout/cart&remove
            if (remove == null)
            {
                //Busca cualquier elemento que pueda ser:link, botón o input
                //Luego revisa si contiene palabras como:remove delete
                                var xpath = "//*[self::a or self::button or self::input]" +
                            "[contains(translate(@title,'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(normalize-space(.),'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(@href,'REMOVEDELETE','removedelete'),'remove') " +
                            "or contains(translate(@href,'REMOVEDELETE','removedelete'),'delete') " +
                            "or contains(translate(@value,'REMOVEDELETE','removedelete'),'remove')]";

                remove = driver.FindElements(By.XPath(xpath)).FirstOrDefault(e => e.Displayed && e.Enabled);//Luego revisa si alguno de esos elementos
                                                                                                            //es visible y habilitado, y toma el primero.
            }

            //Muestra información útil para debug.
            if (remove == null)
            {
                Console.WriteLine("CART URL: " + driver.Url);
                Console.WriteLine("CART TITLE: " + driver.Title);
                //Luego lanza excepción.
                throw new NoSuchElementException("No se encontró botón/link Remove/Delete visible en el carrito.");
            }

            // click seguro
            //A veces Selenium no puede hacer click normal, entonces usa JavaScript click,
            //Y es una técnica común en automatización.
            try { remove.Click(); }
            catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", remove); }

            // esperar que ya no haya productos (o que se actualice la página)
            //Tres posibles señales de que el carrito cambió:
            waitLocal.Until(d =>
                d.FindElements(QuantityInputs).Count == 0 //1- ya no hay productos
                || d.PageSource.ToLower().Contains("empty")//2- la página dice "empty"
                || d.Url.ToLower().Contains("checkout/cart")//3- la URL cambió a checkout/cart
            );

            return this;//Permite encadenar métodos.
        }

        public bool IsCartEmpty()
        {
            // criterio 1: no hay inputs quantity
            //Si no hay inputs de cantidad, el carrito está vacío.
            var noQtyInputs = driver.FindElements(QuantityInputs).Count == 0;

            // criterio 2: texto de carrito vacío (fallback)
            //Busca texto en el HTML:shopping cart is empty
            //Esto es un fallback.
            var html = driver.PageSource.ToLower();
            var hasEmptyText =
                html.Contains("your shopping cart is empty") ||
                html.Contains("shopping cart is empty") ||
                html.Contains("empty");

            return noQtyInputs || hasEmptyText;//Si cualquiera de las dos condiciones
                                               //se cumple, el carrito se considera vacío.
        }

        //Este método hace click en el botón checkout.
        public CheckoutGuestPage ProceedToCheckout()
        {
            var btn = wait.Until(d => d.FindElement(CheckoutButton));//Espera que el botón aparezca.
            btn.Click();//Hace click.
            return new CheckoutGuestPage(driver);//Devuelve la siguiente página del flujo.
        }
    }
}
