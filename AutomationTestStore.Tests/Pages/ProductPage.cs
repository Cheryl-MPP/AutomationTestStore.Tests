using AutomationTestStore.Tests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class ProductPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait wait;

    //Name selectors (varían según template/producto)
    private By ProductName1 => By.CssSelector("h1[itemprop='name']");
    private By ProductName2 => By.CssSelector(".productname");
    private By ProductName3 => By.CssSelector("h1");

    //Add to cart selectors (a veces es <a class="cart">, a veces botón)
    private By AddToCartButton1 => By.CssSelector("a.cart");
    private By AddToCartButton2 => By.CssSelector("button.cart");
    private By AddToCartButton3 => By.CssSelector("[title*='Add to Cart'], [title*='Add to cart']");

    public ProductPage(IWebDriver driver)
    {
        _driver = driver;
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
    }

    public string GetName()
    {
        // 1) Confirmar que ya estamos en página de producto (URL usual del sitio)
        wait.Until(d =>
            d.Url.ToLower().Contains("product/product") ||
            d.Url.ToLower().Contains("product_id")
        );

        // 2) Intentar varios selectores para el nombre
        IWebElement? el =
            TryFind(ProductName1) ??
            TryFind(ProductName2) ??
            TryFind(ProductName3);

        if (el == null)
            throw new NoSuchElementException("No se encontró el nombre del producto en la página.");

        var name = el.Text.Trim();

        // a veces h1 trae texto vacío si hay spans; fallback simple
        if (string.IsNullOrWhiteSpace(name))
            name = _driver.Title?.Trim() ?? "";

        return name;
    }

    public CartPage AddToCart()
    {
        // Confirmar que estamos en producto antes de intentar click
        wait.Until(d =>
            d.Url.ToLower().Contains("product/product") ||
            d.Url.ToLower().Contains("product_id")
        );

        IWebElement? btn =
            TryFind(AddToCartButton1) ??
            TryFind(AddToCartButton2) ??
            TryFind(AddToCartButton3);

        if (btn == null)
            throw new NoSuchElementException("No se encontró botón Add to Cart en la página de producto.");

        try { btn.Click(); }
        catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }

        return new CartPage(_driver);
    }

    private IWebElement? TryFind(By by)
    {
        var els = _driver.FindElements(by);
        return els.FirstOrDefault(e => e.Displayed && e.Enabled);
    }
}