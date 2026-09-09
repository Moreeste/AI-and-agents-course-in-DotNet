using BlazorIA.DTOs;

namespace BlazorIA.Servicios.ChatBots
{
    public interface IChatBot
    {
        List<MensajeChatUI> Conversacion { get; }
        bool EstaProcesando { get; }
        SolicitudAprobacionUI? AprobacionPendiente { get; }
        event Action? OnChange;
        void CancelarRespuestaActual();
        Task EnviarMensajeAsync(string textoUsuario, CancellationToken cancellationToken = default);
        Task ResolverAprobacionAsync(bool aprobada, CancellationToken cancellationToken = default);
    }
}
