using System.Text.Json;
//Importa la librería que permite trabajar con JSON en C#
//para leer y convertir datos que vienen en formato JSON”

namespace AutomationTestStore.Tests.Config
{
    //Aquí se crea una propiedad llamada BaseUrl
    //que va a guardar la URL base del sistema
    //que querés probar, que en este caso es:
    //"BaseUrl": "https://www.automationteststore.com/"
    public class TestConfig
    {
        public string BaseUrl { get; set; } = "";//Inicializa la propiedad BaseUrl con una cadena vacía

        private static TestConfig? _instance;//Declara un campo privado y estático llamado _instance
        //y se guarda una única instancia de la configuración


        //Este es el Getter del Instance
        public static TestConfig Instance//Creamos una propiedad estática llamada Instance.
        {
            get
            {
                if (_instance == null)//Si la instancia es nula, si aún no existe una instancia, entonces la crea.
                {
                    var json = File.ReadAllText("testsettings.json");//Lee el contenido del archivo testsettings.json y lo guarda en la variable json
                    _instance = JsonSerializer.Deserialize<TestConfig>(json, new JsonSerializerOptions//Deserializa el contenido JSON en una instancia de TestConfig
                    {
                        PropertyNameCaseInsensitive = true//Configura la deserialización para que no distinga entre mayúsculas y minúsculas en los nombres de las propiedades
                    })!;
                }

                return _instance;//Devuelve la instancia de TestConfig
            }
        }
    }
}