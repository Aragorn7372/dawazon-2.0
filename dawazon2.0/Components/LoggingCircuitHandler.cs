using Microsoft.AspNetCore.Components.Server.Circuits;
using Serilog;

namespace dawazon2._0.Components;

/// <summary>
/// Manejador de circuitos Blazor Server.
/// Loguea cada evento del ciclo de vida del circuito para depuración.
/// </summary>
public class LoggingCircuitHandler : CircuitHandler
{
    private static int _activeCircuits = 0;

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        int active = Interlocked.Increment(ref _activeCircuits);
        Log.Information("🔌 [Blazor] Circuito CONECTADO — Id: {CircuitId} | Activos: {Active}",
            circuit.Id, active);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        int active = Interlocked.Decrement(ref _activeCircuits);
        Log.Warning("🔌 [Blazor] Circuito DESCONECTADO — Id: {CircuitId} | Activos: {Active}",
            circuit.Id, active);
        return Task.CompletedTask;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Information("✅ [Blazor] Circuito ABIERTO — Id: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Log.Warning("❌ [Blazor] Circuito CERRADO — Id: {CircuitId}", circuit.Id);
        return Task.CompletedTask;
    }
}
