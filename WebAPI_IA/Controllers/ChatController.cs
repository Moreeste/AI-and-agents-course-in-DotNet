using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Security.Claims;
using System.Text.Json;
using WebAPI_IA.Data;
using WebAPI_IA.DTOs;
using WebAPI_IA.Entidades;
using WebAPI_IA.Servicios.Chatbots;
using WebAPI_IA.Utilidades;

namespace WebAPI_IA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController(IChatClientFactory chatClientFactory, ChatOptions chatOptions, ApplicationDbContext context) : ControllerBase
    {
        [HttpGet("ObtenerConversaciones")]
        public async Task<ActionResult<List<ConversacionResumenDTO>>> ObtenerConversaciones()
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return Unauthorized();
            }

            var conversaciones = await context.Conversaciones
                .Where(x => x.UsuarioId == usuarioId)
                .OrderByDescending(x => x.FechaActualizacionUtc)
                .Select(x => new ConversacionResumenDTO
                {
                    Id = x.Id,
                    Titulo = x.Titulo
                })
                .ToListAsync();

            return conversaciones;
        }

        [HttpGet("ObtenerConversacion")]
        public async Task<ActionResult<ConversacionDTO>> ObtenerConversacion(Guid id)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return Unauthorized();
            }
            var conversacionDb = await context.Conversaciones
                .Include(x => x.Mensajes.OrderBy(x => x.Orden))
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId);

            if (conversacionDb is null)
            {
                return NotFound();
            }

            var conversacion = new ConversacionDTO
            {
                Id = id,
                Mensajes = conversacionDb.Mensajes
                .Select(x => new MensajeChatUI
                {
                    Rol = x.Rol,
                    Texto = x.Texto
                }).ToList()
            };

            return conversacion;
        }

        [HttpPost("NuevoChat")]
        public async Task<IActionResult> NuevoChat()
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return Unauthorized();
            }

            var id = Guid.NewGuid();
            var ahora = DateTime.UtcNow;

            context.Conversaciones.Add(new ConversacionChat
            {
                Id = id,
                Titulo = "Nuevo chat",
                UsuarioId = usuarioId,
                FechaCreacionUtc = ahora,
                FechaActualizacionUtc = ahora
            });
            await context.SaveChangesAsync();

            return Ok(new { id });
        }

        [HttpPost("Enviar")]
        public async Task Enviar([FromBody] EnviarMensajeDTO dto)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                await EscribirEventoStream(EventoStream.error, new { mensaje = "Debes iniciar sesión" });
                return;
            }

            if (dto.ConversacionId is null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                await EscribirEventoStream(EventoStream.error, new { mensaje = "Debes enviar el Id de la conversación" });
                return;
            }

            var id = dto.ConversacionId.Value;

            var conversacionDbExiste = await context.Conversaciones.AnyAsync(x => x.Id == id && x.UsuarioId == usuarioId);

            if (!conversacionDbExiste)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                await EscribirEventoStream(EventoStream.error, new { mensaje = "La conversación no existe" });
                return;
            }

            Response.ContentType = "application/x-ndjson";
            Response.Headers.CacheControl = "no-cache";

            var chatbot = await CrearChatbot(id, usuarioId);
            
            await EscribirEventoStream(EventoStream.iniciar_conversacion, new { id });

            try
            {
                await chatbot.EnviarMensajesStreamAsync(dto.Texto, async delta =>
                {
                    await EscribirEventoStream(EventoStream.delta, new { texto = delta });
                }, HttpContext.RequestAborted);

                await GuardarMensajes(id, usuarioId, chatbot.Conversacion);
            }
            catch (OperationCanceledException)
            {

            }
        }

        [HttpPost("BorrarConversacion")]
        public async Task<IActionResult> BorrarConversacion([FromBody] Guid id)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return Unauthorized();
            }

            var conversacionesBorradas = await context.Conversaciones
                .Where(x => x.Id == id).ExecuteDeleteAsync();

            if (conversacionesBorradas == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        private async Task GuardarMensajes(Guid conversacionId, string usuarioId, List<MensajeChatUI> mensajes)
        {
            var conversacion = await context.Conversaciones
                .Include(x => x.Mensajes)
                .FirstOrDefaultAsync(x => x.Id == conversacionId && x.UsuarioId == usuarioId);

            if (conversacion is null)
            {
                return;
            }

            var mensajesPersistidos = conversacion.Mensajes.OrderBy(x => x.Orden).ToList();

            for (var indice = mensajesPersistidos.Count; indice < mensajes.Count; indice ++)
            {
                var mensaje = mensajes[indice];

                context.Mensajes.Add(new MensajeChat
                {
                    Id = Guid.NewGuid(),
                    ConversacionChatId = conversacionId,
                    Rol = mensaje.Rol,
                    Texto = mensaje.Texto,
                    Orden = indice
                });
            }

            conversacion.FechaActualizacionUtc = DateTime.UtcNow;
            conversacion.Titulo = ObtenerTitulo(conversacion.Titulo, mensajes);
            await context.SaveChangesAsync();
        }

        private async Task EscribirEventoStream(EventoStream tipo, object datos)
        {
            var evento = JsonSerializer.Serialize(new
            {
                tipo = tipo.ToString(),
                datos
            });

            await Response.WriteAsync(evento + "\n");
            await Response.Body.FlushAsync();
        }

        private async Task<ChatBotReal> CrearChatbot(Guid conversacionId, string usuarioId)
        {
            var mensajes = await context.Mensajes
                .Where(x => x.ConversacionChatId == conversacionId && x.Conversacion.UsuarioId == usuarioId)
                .OrderBy(x => x.Orden)
                .ToListAsync();

            var mensajesConversacionUI = mensajes.Select(x => new MensajeChatUI
            {
                Rol = x.Rol,
                Texto = x.Texto
            });

            return new ChatBotReal(chatClientFactory, chatOptions, mensajesConversacionUI);
        }

        private string? ObtenerUsuarioId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static string ObtenerTitulo(string tituloActual, IEnumerable<MensajeChatUI> mensajes)
        {
            if (tituloActual != "Nuevo chat")
            {
                return tituloActual;
            }

            var primerMensajeUsuario = mensajes.FirstOrDefault(x => x.Rol == RolMensaje.Usuario)?.Texto.Trim();

            if (string.IsNullOrWhiteSpace(primerMensajeUsuario))
            {
                return tituloActual;
            }

            return primerMensajeUsuario.Length <= 60 ? primerMensajeUsuario : primerMensajeUsuario[..60] + "...";
        }
    }
}
