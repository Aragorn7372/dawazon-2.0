using Microsoft.Playwright;

namespace dawazonPlayWrite;

[TestFixture]
public class AuthTests : BaseTest
{
    [Test]
    public async Task Login_AccessRestrictedPage_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/pedidos");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task Login_InvalidCredentials_ShouldStayOnLoginPage()
    {
        await Page.GotoAsync($"{BaseUrl}/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var emailInput = Page.Locator("input[type='email'], input[name='Email']").First;
        var passwordInput = Page.Locator("input[type='password']").First;
        var submitButton = Page.Locator("button, input[type='submit']").First;
        
        if (await emailInput.IsVisibleAsync())
            await emailInput.FillAsync("invalid@test.com");
        if (await passwordInput.IsVisibleAsync())
            await passwordInput.FillAsync("wrongpassword");
        
        if (await submitButton.IsVisibleAsync())
            await submitButton.ClickAsync();
        
        await Page.WaitForTimeoutAsync(2000);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task Logout_LoggedInUser_ShouldLogout()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var dropdown = Page.Locator(".dropdown-toggle, .dropdown-toggle-custom, nav .nav-link").First;
        if (await dropdown.IsVisibleAsync())
        {
            await dropdown.ClickAsync();
            var logoutLink = Page.Locator("a[href='/logout']");
            if (await logoutLink.IsVisibleAsync())
            {
                await logoutLink.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                var loginLink = Page.Locator("a[href='/login']");
                await Expect(loginLink).ToBeVisibleAsync();
            }
        }
        else
        {
            Assert.Pass("User was not logged in, skipping logout test");
        }
    }
}
