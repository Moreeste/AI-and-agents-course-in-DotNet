using BlazorIA.Utilidades;
using Microsoft.Extensions.AI;

namespace BlazorIA.Servicios
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Crear(string modelo)
        {
            var openAiKey = configuration.GetValue<string>("OpenAI_Key");

            var proveedor = ModelosIA.ObtenerProveedor(modelo);

            var cliente = proveedor switch
            {
                "openai" => new OpenAI.Chat.ChatClient(modelo ?? "gpt-5.4-nano", openAiKey).AsIChatClient(),
                _ => throw new ArgumentException($"Proveedor desconocido: {proveedor}"),
            };

            return cliente.AsBuilder()
            .UseFunctionInvocation(null, c =>
            {
                c.IncludeDetailedErrors = true;
            })
            .Build(sp);
        }
    }
}
