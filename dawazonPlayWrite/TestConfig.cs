namespace dawazonPlayWrite;

public class TestConfig
{
    public static string BaseUrl { get; private set; } = "http://127.0.0.1:5041";
    public static bool Headless { get; private set; } = true;
    public static string BrowserType { get; private set; } = "chromium";
    public static int Timeout { get; private set; } = 30000;
    public static bool SlowMo { get; private set; } = false;
    public static int SlowMoDelay { get; private set; } = 0;
    public static bool RecordVideo { get; private set; } = true;
    public static string VideoDir { get; private set; } = "videos";

    static TestConfig()
    {
        LoadFromEnvFile();
        LoadFromEnvironment();
    }

    private static void LoadFromEnvFile()
    {
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.env"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "test.env"),
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "dawazonPlayWrite", "test.env"),
            Path.Combine(Directory.GetCurrentDirectory(), "dawazonPlayWrite", ".env"),
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\dawazonPlayWrite\test.env",
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\dawazonPlayWrite\.env",
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\test.env",
            @"C:\Users\sggz2\RiderProjects\dawazon-2.0\.env"
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

    public static void LoadFromEnvironment()
    {
        var baseUrlEnv = Environment.GetEnvironmentVariable("DAWAZON_BASE_URL");
        if (!string.IsNullOrEmpty(baseUrlEnv))
        {
            BaseUrl = baseUrlEnv.TrimEnd('/');
        }

        var headlessEnv = Environment.GetEnvironmentVariable("DAWAZON_HEADLESS");
        if (!string.IsNullOrEmpty(headlessEnv))
        {
            Headless = !headlessEnv.Equals("false", StringComparison.OrdinalIgnoreCase) &&
                       !headlessEnv.Equals("0", StringComparison.OrdinalIgnoreCase) &&
                       !headlessEnv.Equals("no", StringComparison.OrdinalIgnoreCase);
        }

        var browserEnv = Environment.GetEnvironmentVariable("DAWAZON_BROWSER");
        if (!string.IsNullOrEmpty(browserEnv))
        {
            BrowserType = browserEnv.ToLower();
        }

        var timeoutEnv = Environment.GetEnvironmentVariable("DAWAZON_TIMEOUT");
        if (int.TryParse(timeoutEnv, out var timeout))
        {
            Timeout = timeout;
        }

        var slowMoEnv = Environment.GetEnvironmentVariable("DAWAZON_SLOWMO");
        if (!string.IsNullOrEmpty(slowMoEnv))
        {
            SlowMo = slowMoEnv.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                     slowMoEnv.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                     slowMoEnv.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        var slowMoDelayEnv = Environment.GetEnvironmentVariable("DAWAZON_SLOWMO_DELAY");
        if (int.TryParse(slowMoDelayEnv, out var delay))
        {
            SlowMoDelay = delay;
        }

        var recordVideoEnv = Environment.GetEnvironmentVariable("DAWAZON_RECORD_VIDEO");
        if (!string.IsNullOrEmpty(recordVideoEnv))
        {
            RecordVideo = recordVideoEnv.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                          recordVideoEnv.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                          recordVideoEnv.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        var videoDirEnv = Environment.GetEnvironmentVariable("DAWAZON_VIDEO_DIR");
        if (!string.IsNullOrEmpty(videoDirEnv))
        {
            VideoDir = videoDirEnv;
        }
    }

    public static void Reload()
    {
        LoadFromEnvFile();
        LoadFromEnvironment();
    }
}
