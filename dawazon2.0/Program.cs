using System.Text;
using dawazon2._0.Components;
using dawazon2._0.Infraestructures;
using dawazon2._0.Middleware;
using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger= SerilogConfig.Configure().CreateLogger();
Console.OutputEncoding = Encoding.UTF8;
var builder = WebApplication.CreateBuilder(args);
// creo variables para que sea mas facil de leer
var services = builder.Services;
var configuration = builder.Configuration;
var environment = builder.Environment;
// añado configuracion de serilog para que se habilite en todos los logger
builder.Host.UseSerilog();
// añado configuracion de controllers
services.AddMvcControllers();
// añado razorpages
services.AddRazorPages();
// añado la base de datos
services.AddDatabase(configuration);
// politicas de corps
services.AddCorsPolicy(configuration,true);
// limite de peticiones 
services.AddRateLimitingPolicy();
// añade autorizacion
services.AddAuthorization();
// añade autenticacion
services.AddAuthentication(configuration);
// añado la cache
services.AddCache(configuration);
// añade las sesiones para paginas din
services.AddSession(configuration);
// añado repositorios
services.AddRepositories();
// añado servicios
services.AddServices();
//añado email service
services.AddEmail(environment);
// añado storageService
services.AddStorage();
// tarea programada: limpia carritos con checkout expirado cada 2 minutos
services.AddHostedService<CartCleanupBackgroundService>();
// añado configuracion de MVC
services.AddControllersWithViews();
services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true; 
});
services.AddScoped<CircuitHandler, LoggingCircuitHandler>();
// declaro app
var app = builder.Build(); 
// en produccion mapea los errores web
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseStatusCodePagesWithReExecute("/NotFound", "?code={0}");
}
// global exception handler
app.UseGlobalExceptionHandler();
// politicas de corps (ANTES de routing para que las pre-flights pasen)
app.UseCorsPolicy();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// archivos estaticos ANTES de routing (blazor.server.js, etc.)
app.UseStaticFiles();
app.UseRouting();
// las sesiones deben estar disponibles ANTES de que la autenticación intente leer la cookie
app.UseSession();
// lo que tiene relacion con usuarios
app.UseAuthentication();
app.UseAuthorization();
// mapeador de controllers
app.MapControllers();
// mapeador de rutas convencionales MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// mapeador de razor pages
app.MapRazorPages();

app.MapBlazorHub();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
// init de datos
await app.SeedIdentityAsync();
await app.InitializeDatabaseAsync();
// init del storage
app.InitializeStorage();

Log.Information("=== CONFIGURATION VALUES ===");
Log.Information("Storage: UploadPath={UploadPath}, MaxFileSize={MaxFileSize}, AllowedExtensions={AllowedExtensions}, AllowedContentTypes={AllowedContentTypes}",
    configuration["Storage:UploadPath"], configuration["Storage:MaxFileSize"], configuration["Storage:AllowedExtensions"], configuration["Storage:AllowedContentTypes"]);
Log.Information("Stripe: Key={StripeKey}", configuration["Stripe:Key"]);
Log.Information("Server: Url={ServerUrl}", configuration["Server:Url"]);
Log.Information("Development: {Development}", configuration["Development"]);
Log.Information("Jwt: Key={JwtKey}, Issuer={JwtIssuer}, Audience={JwtAudience}", 
    configuration["Jwt:Key"], configuration["Jwt:Issuer"], configuration["Jwt:Audience"]);
Log.Information("Smtp: Host={SmtpHost}, Port={SmtpPort}, Username={SmtpUsername}, AdminEmail={SmtpAdminEmail}",
    configuration["Smtp:Host"], configuration["Smtp:Port"], configuration["Smtp:Username"], configuration["Smtp:AdminEmail"]);
Log.Information("ConnectionStrings: DefaultConnection={DefaultConnection}", configuration["ConnectionStrings:DefaultConnection"]);
Log.Information("Redis: Host={RedisHost}, Password={RedisPassword}, Port={RedisPort}",
    configuration["Redis:Host"], configuration["Redis:Password"], configuration["Redis:Port"]);
Log.Information("=== END CONFIGURATION ===");

try
{
    Log.Information("Iniciando aplicación FunkoApi...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}