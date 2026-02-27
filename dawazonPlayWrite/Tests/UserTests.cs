using Microsoft.Playwright;

namespace dawazonPlayWrite;

[TestFixture]
public class UserTests : BaseTest
{
    [Test]
    public async Task UserProfile_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/perfil");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task UserFavorites_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/perfil/favoritos");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }

    [Test]
    public async Task UserEditProfile_WithoutLogin_ShouldRedirect()
    {
        await Page.GotoAsync($"{BaseUrl}/perfil/editar");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("/login"), Is.True);
    }
}
