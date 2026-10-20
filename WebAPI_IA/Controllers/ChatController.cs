using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Security.Claims;
using WebAPI_IA.Data;
using WebAPI_IA.DTOs;
using WebAPI_IA.Entidades;
using WebAPI_IA.Servicios.Chatbots;

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
        public async Task<ActionResult<string>> Enviar([FromBody] EnviarMensajeDTO dto)
        {
            var usuarioId = ObtenerUsuarioId();

            if (usuarioId is null)
            {
                return "Debes iniciar sesión";
            }

            if (dto.ConversacionId is null)
            {
                return "Debes enviar el Id de la conversación";
            }

            var id = dto.ConversacionId.Value;

            var conversacionDbExiste = await context.Conversaciones.AnyAsync(x => x.Id == id && x.UsuarioId == usuarioId);

            if (!conversacionDbExiste)
            {
                return "La conversación no existe";
            }

            var chatbot = await CrearChatbot(id, usuarioId);
            var respuestaIA = string.Empty;

            try
            {
                await chatbot.EnviarMensajesStreamAsync(dto.Texto, async delta =>
                {

                }, HttpContext.RequestAborted);

                await GuardarMensajes(id, usuarioId, chatbot.Conversacion);
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                respuestaIA = chatbot.Conversacion[^1].Texto;
            }

            return respuestaIA;
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
