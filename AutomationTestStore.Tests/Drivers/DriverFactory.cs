using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace AutomationTestStore.Tests.Drivers
{
    public static class DriverFactory
    {
        private static readonly AsyncLocal<IWebDriver?> _driver = new();//Aquí se almacena el navegador de Selenium.
        //usa AsyncLocal porque permite que cada flujo de ejecución tenga su propio driver, en vez de compartir uno solo globalmente.
        public static IWebDriver Driver =>//Expone el driver actual de forma segura.
            _driver.Value ?? throw new InvalidOperationException("Driver no inicializado. Llamá InitDriver() primero.");
        //te devuelve el navegador actual, pero si todavía no se ha inicializado, lanza un error claro...

        //Este método crea e inicializa el navegador.
        public static void InitDriver()
        {
            if (_driver.Value != null) return;//Si ya existe un driver, no crea otro.

            var options = new ChromeOptions();//Crea opciones para Chrome y le indica que se abra maximizado
            options.AddArgument("--start-maximized");
            //Basicamente abre el navegador en pantalla completa o maximizado
            //para que los elementos tengan mejor visibilidad y el comportamiento
            //sea más estable.

            // Eager ayuda MUCHO a que no se quede pegado esperando load completo
            //le dice a Selenium que no espere a que absolutamente todo termine
            //de cargar para continuar.
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            // (Opcional) estabilidad
            //Desactiva notificaciones y algunos bloqueos de popups
            //reduce interrupciones molestas durante la automatización
            //eso ayuda a que no te salgan ventanitas o comportamientos
            //raros que afecten el test
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");


            //Crea el servicio que ejecuta ChromeDriver y
            //oculta la ventana negra de consola
            //o sea hace que el ChromeDriver corra
            //“por debajo” sin mostrar la consola extra
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            //Crea la instancia real del navegador Chrome con las opciones configuradas
            //el command time es el tiempo máximo que Selenium espera para comandos
            //enviados al driver antes de considerar que hubo timeout.
            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(180));

            // Timeouts de Selenium (independientes al command timeout)
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);//Desactiva la espera implícita, le dice a Selenium que no espere automáticamente al buscar elementos.
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);//Si una página dura más de 60 segundos en cargar, Selenium lanza timeout ya define cuánto tiempo puede esperar Selenium a que cargue una página.
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(60);//Selenium no esperará más de 60 segundos.

            _driver.Value = driver;//Guarda la instancia recién creada en _driver
        }

        public static void QuitDriver()//Cierra y libera el navegador al final.
        {
            try { _driver.Value?.Quit(); } catch { }//Cierra todas las ventanas del navegador y termina la sesión de Selenium.
            try { _driver.Value?.Dispose(); } catch { }//Libera recursos del objeto driver.
            _driver.Value = null;//Borra la referencia guardada, dejando listo el sistema para una nueva inicialización futura.
        }// se encuentran dentro de un try porque a veces el navegador ya está cerrado o se rompió la sesión
        //entonces se evita que el test truene solo por intentar cerrar algo que ya no responde.
    }
}