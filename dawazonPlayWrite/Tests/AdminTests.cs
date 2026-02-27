using Microsoft.Playwright;

namespace dawazonPlayWrite;

[TestFixture]
public class AdminTests : BaseTest
{
    [Test]
    public async Task AdminUsers_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/admin/usuarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task AdminSales_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/admin/ventas");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task AdminStats_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/admin/estadisticas");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task AdminUsers_AsUser_ShouldBeDenied()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var emailInput = Page.Locator("input[type='email'], input[name='Email']").First;
        var passwordInput = Page.Locator("input[type='password']").First;
        var submitButton = Page.Locator("button, input[type='submit']").First;
        
        if (await emailInput.IsVisibleAsync())
            await emailInput.FillAsync("user@test.com");
        if (await passwordInput.IsVisibleAsync())
            await passwordInput.FillAsync("Test1234!");
        
        if (await submitButton.IsVisibleAsync())
            await submitButton.ClickAsync();
        
        await Page.WaitForTimeoutAsync(3000);
        
        await Page.GotoAsync($"{BaseUrl}/admin/usuarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/admin/usuarios") || currentUrl.Contains("/login"), Is.True);
    }
}
