using BlazorIA.DTOs;
using Microsoft.Extensions.AI;

namespace BlazorIA.Servicios.ChatBots
{
    public class ChatBotReal : IChatBot
    {
        private readonly IChatClient _cliente;
        private readonly List<ChatMessage> mensajes = [];

        public List<MensajeChatUI> Conversacion { get; } = [];
        public bool EstaProcesando { get; private set; }
        public event Action? OnChange;

        public ChatBotReal(IChatClient cliente)
        {
            _cliente = cliente;

            var systemPromptGeneral = """
            Eres un asistente que responde preguntas generales.
            Debes responder en español.
            Las respuestas deben ser en texto plano, no usar formatos como markdown.
            Las respuestas deben ser concisas a menos que te indiquen lo contrario.

            Si un tool falla, lee el mensaje de la excepción para ver si puedes arreglarlo haciendo algún ajuste. Comunícale al usuario cualquier ajuste que vayas a hacer.
            """;

            mensajes.Add(new ChatMessage(ChatRole.System, systemPromptGeneral));
        }

        public void CancelarRespuestaActual()
        {

        }

        public async Task EnviarMensajeAsync(string textoUsuario, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(textoUsuario))
            {
                return;
            }

            if (EstaProcesando)
            {
                return;
            }

            EstaProcesando = true;

            Conversacion.Add(new MensajeChatUI
            {
                Rol = RolMensaje.Usuario,
                Texto = textoUsuario
            });

            mensajes.Add(new ChatMessage(ChatRole.User, textoUsuario));

            Conversacion.Add(new MensajeChatUI
            {
                Rol = RolMensaje.IA,
                Texto = string.Empty
            });

            NotificarCambio();
            await ProcesarRespuesta(cancellationToken);
            EstaProcesando = false;
        }

        private async Task ProcesarRespuesta(CancellationToken cancellationToken)
        {
            var updates = new List<ChatResponseUpdate>();

            await foreach (var update in _cliente.GetStreamingResponseAsync(mensajes, cancellationToken: cancellationToken))
            {
                updates.Add(update);

                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        Conversacion[^1].Texto += textContent.Text;
                        NotificarCambio();
                    }
                }

                var respuesta = updates.ToChatResponse();
                mensajes.AddMessages(respuesta);
            }
        }

        private void NotificarCambio() => OnChange?.Invoke();

        public Task ResolverAprobacionAsync(bool aprobada, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
