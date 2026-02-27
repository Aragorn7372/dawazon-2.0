using Microsoft.Playwright;

namespace dawazonPlayWrite;

[TestFixture]
public class ManagerTests : BaseTest
{
    [Test]
    public async Task ManagerSales_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/manager/ventas");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task ManagerSalesEdit_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/manager/ventas/123/editar/456");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task ManagerCreateProduct_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/crear");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }
}
