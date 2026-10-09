using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using WebAPI_IA.DTOs;
using WebAPI_IA.Servicios.Chatbots;

namespace WebAPI_IA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IChatClientFactory chatClientFactory, ChatOptions chatOptions) : ControllerBase
    {
        [HttpPost("Enviar")]
        public async Task<ActionResult<string>> Enviar([FromBody] EnviarMensajeDTO dto)
        {
            var chatbot = await CrearChatbot();
            var respuestaIA = string.Empty;

            try
            {
                await chatbot.EnviarMensajesStreamAsync(dto.Texto, async delta =>
                {

                }, HttpContext.RequestAborted);
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

        private async Task<ChatBotReal> CrearChatbot()
        {
            return new ChatBotReal(chatClientFactory, chatOptions, []);
        }
    }
}
