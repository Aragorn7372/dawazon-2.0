using Microsoft.AspNetCore.SignalR;

namespace dawazonBackend.Common.Hub;

/// <summary>
/// Hub de SignalR para notificaciones en tiempo real.
/// </summary>
/// <remarks>
/// Este Hub permite la comunicación bidireccional en tiempo real entre el servidor
/// y los clientes conectados. Se utiliza para enviar notificaciones instantáneas
/// a los clientes sin necesidad de polling.
///
/// <para><b>Funcionalidades:</b></para>
/// <list type="bullet">
///     <item>Notificaciones en tiempo real</item>
///     <item>Actualización de estado de pedidos</item>
///     <item>Alertas de stock</item>
///     <item>Mensajería entre usuarios</item>
/// </list>
/// 
/// <para><b>Protocolo:</b></para>
/// Los clientes se conectan a este hub y pueden suscribirse a grupos o recibir
/// mensajes directamente.
///
/// <para><b>Configuración:</b></para>
/// En Program.cs:
/// <code>app.MapHub&lt;NotificationHub&gt;("/hubs/notifications");</code>
/// </remarks>
public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
{
    /// <summary>
    /// Constructor del Hub.
    /// </summary>
    /// <remarks>
    /// Hereda funcionalidad base de Hub de SignalR.
    /// </remarks>
}