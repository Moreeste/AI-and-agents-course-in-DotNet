using BlazorIA.DTOs;
using BlazorIA.RAG.Servicios;
using BlazorIA.Servicios;
using BlazorIA.Servicios.ChatBots;
using BlazorIA.Utilidades;
using Microsoft.Extensions.AI;

namespace BlazorIA.RAG.ChatBots
{
    public class ChatBotRag : IChatBot
    {
        private string modelo;
        private readonly IChatClientFactory chatClientFactory;
        private readonly ChatOptions chatOptions;
        private readonly IServicioRag servicioRag;
        private readonly List<ChatMessage> mensajes = [];
        private readonly Queue<ToolApprovalRequestContent> aprobacionesPendientes = new();
        private CancellationTokenSource? _ctsActual;

        public List<MensajeChatUI> Conversacion { get; } = [];
        public bool EstaProcesando { get; private set; }
        public event Action? OnChange;
        public SolicitudAprobacionUI? AprobacionPendiente { get; private set; }

        public ChatBotRag(IChatClientFactory chatClientFactory, ChatOptions chatOptions, IServicioRag servicioRag)
        {
            modelo = ModelosIA.ObtenerModeloPorDefecto;
            this.chatClientFactory = chatClientFactory;
            this.chatOptions = chatOptions;
            this.servicioRag = servicioRag;
            var systemPromptGeneral = """
            Eres un asistente que responde preguntas sobre documentos internos de una empresa
            Debes responder en español.
            Las respuestas deben ser concisas a menos que te indiquen lo contrartio.
            Las respuestas deben ser en texto plano, no usar markdown.

            Usa prioritariamente el contexto recuperado de los documentos.
            Si la respuesta no está en el contexto, dilo claramente.
            No inventes políticas, procesos ni datos que no aparezcan en el contenido.
            """;

            mensajes.Add(new ChatMessage(ChatRole.System, systemPromptGeneral));
        }

        public void CancelarRespuestaActual()
        {
            if (EstaProcesando)
            {
                _ctsActual?.Cancel();
            }
        }

        public async Task EnviarMensajeAsync(string textoUsuario, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(textoUsuario))
            {
                return;
            }

            if (EstaProcesando || AprobacionPendiente is not null)
            {
                return;
            }

            try
            {
                EstaProcesando = true;
                _ctsActual = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

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
                await ProcesarRespuesta(textoUsuario, _ctsActual.Token);
            }
            catch (OperationCanceledException)
            {
                ManejarOperacionCancelada();
            }
            finally
            {
                ManejarFinally();
            }
        }

        private void ManejarOperacionCancelada()
        {
            if (Conversacion.Count > 0 && Conversacion[^1].Rol == RolMensaje.IA)
            {
                if (string.IsNullOrWhiteSpace(Conversacion[^1].Texto))
                {
                    Conversacion[^1].Texto = "[Respuesta cancelada]";
                }
                else
                {
                    Conversacion[^1].Texto += " [cancelado]";
                }
            }
        }

        private void ManejarFinally()
        {
            _ctsActual?.Dispose();
            _ctsActual = null;
            EstaProcesando = false;
            NotificarCambio();
        }

        private async Task ProcesarRespuesta(string textoUsuario, CancellationToken cancellationToken)
        {
            var contexto = await servicioRag.BuscarContextoRelevante(textoUsuario, top: 3, cancellationToken);

            var mensajeContexto = new ChatMessage(ChatRole.System,
                $"""
                Contexto recuperado de la base documental: 
                {string.Join("\n\n--\n\n", contexto)}
                """);

            var mensajesParaEnviar = new List<ChatMessage>();
            mensajesParaEnviar.AddRange(mensajes);
            mensajesParaEnviar.Insert(mensajesParaEnviar.Count - 1, mensajeContexto);

            var updates = new List<ChatResponseUpdate>();

            var cliente = chatClientFactory.Crear(modelo);

            await foreach (var update in cliente.GetStreamingResponseAsync(mensajesParaEnviar, chatOptions, cancellationToken: cancellationToken))
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
            }

            var respuesta = updates.ToChatResponse();
            mensajes.AddMessages(respuesta);

            var solicitudesAprobacion = respuesta.Messages
                .SelectMany(m => m.Contents)
                .OfType<ToolApprovalRequestContent>()
                .ToList();

            if (solicitudesAprobacion.Count > 0)
            {
                foreach (var solicitud in solicitudesAprobacion)
                {
                    aprobacionesPendientes.Enqueue(solicitud);
                }
                
                //Removemos el mensaje vacío de la IA.
                if (string.IsNullOrEmpty(Conversacion[^1].Texto))
                {
                    Conversacion.RemoveAt(Conversacion.Count - 1);
                }

                MostrarSiguienteAprobacionPendiente();
                NotificarCambio();
                return;
            }
        }

        private void MostrarSiguienteAprobacionPendiente()
        {
            if (aprobacionesPendientes.Count == 0)
            {
                AprobacionPendiente = null;
                return;
            }

            var solicitudAprobacion = aprobacionesPendientes.Dequeue();

            if (solicitudAprobacion.ToolCall is FunctionCallContent functionCall)
            {
                AprobacionPendiente = new SolicitudAprobacionUI
                {
                    SolicitudAprobacion = solicitudAprobacion,
                    NombreTool = ConvertirNombreDeFuncion(functionCall.Name),
                    Argumentos = functionCall.Arguments?.ToDictionary(x => x.Key, x => x.Value) ?? []
                };
            }
        }

        private void NotificarCambio() => OnChange?.Invoke();

        public Task ResolverAprobacionAsync(bool aprobada, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private static string ConvertirNombreDeFuncion(string nombre)
        {
            return nombre switch
            {
                "EnviarCorreo" => "Enviar correo",
                _ => nombre
            };
        }

        public void SetearModelo(string modelo)
        {
            this.modelo = modelo;
        }
    }
}
