using dawazonBackend.Cart.Service;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Servicio en segundo plano para limpieza de carritos expirados.
/// </summary>
/// <remarks>
/// Limpia periódicamente los carritos cuyo checkout lleva más de 5 minutos sin completarse.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Intervalo: cada 2 minutos</item>
///     <item>Expiración: 5 minutos</item>
///     <item>Usa IServiceScopeFactory para servicios scoped</item>
/// </list>
/// </remarks>
public class CartCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<CartCleanupBackgroundService> logger) : BackgroundService
{
    /// <summary>Intervalo de ejecución: cada 2 minutos.</summary>
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    /// <summary>Tiempo máximo desde el inicio del checkout antes de considerar el carrito expirado.</summary>
    private const int ExpirationMinutes = 5;

    /// <summary>
    /// Ejecuta la limpieza de carritos expirados.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelación.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CartCleanupBackgroundService iniciado (intervalo: {Interval} min, expiración: {Exp} min).",
            Interval.TotalMinutes, ExpirationMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                // ICartService es Scoped → creamos un scope para cada ejecución
                await using var scope = scopeFactory.CreateAsyncScope();
                var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

                logger.LogDebug("Ejecutando limpieza de carritos expirados...");
                await cartService.CleanupExpiredCheckoutsAsync(ExpirationMinutes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante la limpieza de carritos expirados.");
            }
        }

        logger.LogInformation("CartCleanupBackgroundService detenido.");
    }
}
