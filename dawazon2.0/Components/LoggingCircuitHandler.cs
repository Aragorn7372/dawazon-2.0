using Microsoft.AspNetCore.Components.Server.Circuits;
using Serilog;

namespace dawazon2._0.Components;

/// <summary>
/// Manejador de circuitos Blazor Server.
/// </summary>
/// <remarks>
/// Registra eventos del ciclo de vida de circuitos SignalR para debugging.
/// 
/// <para><b>Circuitos monitoreados:</b></para>
/// <list type="bullet">
///     <item>Conexión establecida</item>
///     <item>Conexión perdida</item>
///     <item>Circuito abierto</item>
///     <item>Circuito cerrado</item>
/// </list>
/// 
/// <para><b>Utilidad:</b></para>
/// <list type="bullet">
///     <item>Monitoreo de clientes Blazor activos</item>
///     <item>Depuración de desconexiones</item>
///     <item>Logging con Serilog</item>
/// </list>
/// </remarks>
public class LoggingCircuitHandler : CircuitHandler
{
    private static int _activeCircuits;

    /// <summary>
    /// Se ejecuta cuando un circuito se conecta exitosamente.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Incrementa el contador de circuitos activos</item>
    ///     <item>Loguea información de conexión con ID del circuito</item>
    /// </list>
    /// </remarks>
    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        int active = Interlocked.Increment(ref _activeCircuits);
        Log.Information("🔌 [Blazor] Circuito CONECTADO — Id: {CircuitId} | Activos: {Active}",
            circuit.Id, active);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Se ejecuta cuando un circuito se desconecta.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Decrementa el contador de circuitos activos</item>
    ///     <item>Loguea advertencia de desconexión</item>
    /// </list>
    /// </remarks>
    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        int active = Interlocked.Decrement(ref _activeCircuits);
        Log.Warning("[Blazor] Circuito DESCONECTADO — Id: {CircuitId} | Activos: {Active}",
            circuit.Id, active);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Se ejecuta cuando un circuito se abre.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Loguea evento de apertura de circuito</item>
    /// </list>
    /// </remarks>
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)

    {
        Log.Information("[Blazor] Circuito ABIERTO — Id: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Se ejecuta cuando un circuito se cierra.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Loguea evento de cierre de circuito</item>
    /// </list>
    /// </remarks>
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Warning("[Blazor] Circuito CERRADO — Id: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }
}
