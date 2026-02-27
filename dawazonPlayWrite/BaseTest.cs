using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace dawazonPlayWrite;

[SetUpFixture]
public class PlaywrightSetup
{
    public static IPlaywright? Playwright;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        LoadEnvFile();
        TestConfig.Reload();
        
        Console.WriteLine($"=== Playwright Configuration ===");
        Console.WriteLine($"Base URL: {TestConfig.BaseUrl}");
        Console.WriteLine($"Headless: {TestConfig.Headless}");
        Console.WriteLine($"Browser: {TestConfig.BrowserType}");
        Console.WriteLine($"Record Video: {TestConfig.RecordVideo}");
        Console.WriteLine($"Video Dir: {TestConfig.VideoDir}");
        Console.WriteLine($"================================");
        
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
    }

    private static void LoadEnvFile()
    {
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "dawazonPlayWrite", ".env"),
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\dawazonPlayWrite\.env"
        };

        foreach (var envFilePath in paths)
        {
            if (File.Exists(envFilePath))
            {
                Console.WriteLine($"Loading .env from: {envFilePath}");
                foreach (var line in File.ReadAllLines(envFilePath))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        continue;

                    var eqIndex = trimmedLine.IndexOf('=');
                    if (eqIndex > 0)
                    {
                        var key = trimmedLine.Substring(0, eqIndex).Trim();
                        var value = trimmedLine.Substring(eqIndex + 1).Trim();
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
                break;
            }
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        Playwright?.Dispose();
    }
}

public abstract class BaseTest : PageTest
{
    protected string BaseUrl => TestConfig.BaseUrl;
    protected bool Headless => TestConfig.Headless;
    protected int Timeout => TestConfig.Timeout;
    protected bool RecordVideo => TestConfig.RecordVideo;
    protected string VideoDir => TestConfig.VideoDir;

    public override BrowserNewContextOptions ContextOptions()
    {
        var videoPath = Path.Combine(Directory.GetCurrentDirectory(), VideoDir);
        
        if (RecordVideo && !Directory.Exists(videoPath))
        {
            Directory.CreateDirectory(videoPath);
            Console.WriteLine($"Creating video directory: {videoPath}");
        }

        var options = new BrowserNewContextOptions()
        {
            IgnoreHTTPSErrors = true,
            AcceptDownloads = true
        };

        if (RecordVideo)
        {
            options.RecordVideoDir = videoPath;
            Console.WriteLine($"Video recording enabled. Directory: {videoPath}");
        }

        return options;
    }

    [SetUp]
    public async Task SetupTest()
    {
        LoadEnvFile();
        TestConfig.Reload();
        Console.WriteLine($"[TEST] BaseUrl being used: {BaseUrl}");
    }

    [TearDown]
    public async Task TearDownTest()
    {
        if (RecordVideo)
        {
            await Page.WaitForTimeoutAsync(3000);
        }
    }

    protected async Task LoginAsUserAsync(string email, string password)
    {
        await Page.GotoAsync($"{BaseUrl}/Auth/Login");
        await Page.FillAsync("input[name='Email']", email);
        await Page.FillAsync("input[name='Password']", password);
        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    protected async Task LoginAsManagerAsync(string email, string password)
    {
        await LoginAsUserAsync(email, password);
    }

    protected async Task LoginAsAdminAsync(string email, string password)
    {
        await LoginAsUserAsync(email, password);
    }

    protected async Task LogoutAsync()
    {
        await Page.ClickAsync(".dropdown-toggle-custom");
        await Page.ClickAsync("a[href='/Auth/Logout']");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    protected async Task<bool> IsLoggedInAsync()
    {
        return await Page.Locator(".dropdown-toggle-custom").IsVisibleAsync();
    }

    protected async Task<string> GetUserRoleAsync()
    {
        var userInfo = await Page.Locator(".dropdown-toggle-custom").InnerTextAsync();
        if (userInfo.Contains("Admin"))
            return "Admin";
        if (userInfo.Contains("Manager"))
            return "Manager";
        return "User";
    }

    protected async Task WaitForToastAsync(string? expectedMessage = null)
    {
        if (expectedMessage != null)
        {
            await Page.Locator($".alert:has-text('{expectedMessage}')").First.WaitForAsync();
        }
        else
        {
            await Page.Locator(".alert").First.WaitForAsync();
        }
    }

    protected string GetCurrentUrl()
    {
        return Page.Url;
    }

    private static void LoadEnvFile()
    {
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "dawazonPlayWrite", ".env"),
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\dawazonPlayWrite\.env"
        };

        foreach (var envFilePath in paths)
        {
            if (File.Exists(envFilePath))
            {
                foreach (var line in File.ReadAllLines(envFilePath))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        continue;

                    var eqIndex = trimmedLine.IndexOf('=');
                    if (eqIndex > 0)
                    {
                        var key = trimmedLine.Substring(0, eqIndex).Trim();
                        var value = trimmedLine.Substring(eqIndex + 1).Trim();
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
                break;
            }
        }
    }
}
