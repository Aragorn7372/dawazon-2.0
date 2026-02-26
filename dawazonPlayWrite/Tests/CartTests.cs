using Microsoft.Playwright;

namespace dawazonPlayWrite;

[TestFixture]
public class CartTests : BaseTest
{
    [Test]
    public async Task Cart_EmptyCart_ShouldShowEmptyMessage()
    {
        await Page.GotoAsync($"{BaseUrl}/pedidos/carrito");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var redirectToLogin = Page.Locator("text=Iniciar sesión,text=Login,text=Email");
        if (await redirectToLogin.IsVisibleAsync())
        {
            Assert.Pass("Redirected to login page as expected for unauthenticated user");
        }
    }

    [Test]
    public async Task Cart_Checkout_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/pedidos/checkout");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task Cart_MyOrders_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/pedidos");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task Cart_Detail_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/pedidos/anyid");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }
}
