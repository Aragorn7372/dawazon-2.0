using System.Text;
using dawazon2._0.Infraestructures;
using dawazon2._0.Middleware;
using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
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
// configuro identity
services.AddIdentity<User, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<DawazonDbContext>()
    .AddDefaultTokenProviders();
// politicas de corps
services.AddCorsPolicy(configuration,true);
// limite de peticiones 
services.AddRateLimitingPolicy();
// añade autorizacion
services.AddAuthorization();
// añade autenticacion
services.AddAuthentication();
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
// añado configuracion de MVC
services.AddControllersWithViews();
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
// politicas de corps
app.UseCorsPolicy();
app.UseHttpsRedirection();
app.UseRouting();
// lo que tiene relacion con usuarios
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
// uso de archivos estaticos
app.UseStaticFiles();
// mapeador de controllers
app.MapControllers();
// init de datos
await app.InitializeDatabaseAsync();
await app.SeedIdentityAsync();
// init del storage
app.InitializeStorage();
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