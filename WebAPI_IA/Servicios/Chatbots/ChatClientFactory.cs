using Microsoft.Extensions.AI;

namespace WebAPI_IA.Servicios.Chatbots
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Crear()
        {
            var openAiKey = configuration.GetValue<string>("OpenAI_Key");
            var openAiModel = configuration.GetValue<string>("OpenAI_Model");

            var cliente = new OpenAI.Chat.ChatClient(openAiModel ?? "gpt-5.4-nano", openAiKey).AsIChatClient();

            return cliente.AsBuilder()
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
            .Build(sp);
        }
    }
}
