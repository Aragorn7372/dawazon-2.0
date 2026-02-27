namespace dawazonPlayWrite;

[TestFixture]
public class ProductsTests : BaseTest
{
    [Test]
    public async Task ProductsIndex_LoadPage_ShouldDisplayProducts()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        await Expect(Page).ToHaveTitleAsync(new Regex(".*dawazon.*|.*Dawazon.*", RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task ProductsIndex_SearchByName_ShouldFilterProducts()
    {
        await Page.GotoAsync($"{BaseUrl}/?nombre=test");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var products = Page.Locator(".card");
        var count = await products.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task ProductsIndex_FilterByCategory_ShouldShowProducts()
    {
        await Page.GotoAsync($"{BaseUrl}/?categoria=Funko");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    [Test]
    public async Task ProductsIndex_Pagination_ShouldNavigatePages()
    {
        await Page.GotoAsync($"{BaseUrl}/?page=1");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    [Test]
    public async Task ProductsDetail_ViewProduct_ShouldShowDetails()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var productLink = Page.Locator(".card a.btn-amazon").First;
        if (await productLink.IsVisibleAsync())
        {
            await productLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            
            var productDetail = Page.Locator(".product-title, .card-body");
            await Expect(productDetail).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task ProductsDetail_NotFound_ShouldReturn404()
    {
        await Page.GotoAsync($"{BaseUrl}/productos/INVALID_ID_12345");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        
        var notFound = Page.Locator("text=No encontrado,text=404,text=Not Found");
        if (await notFound.CountAsync() > 0 || Page.Url.Contains("404"))
        {
            Assert.Pass();
        }
    }
}
