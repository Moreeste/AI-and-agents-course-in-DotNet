using BlazorIA.Utilidades;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace BlazorIA.Servicios
{
    public class ChatClientFactory(IConfiguration configuration, IServiceProvider sp) : IChatClientFactory
    {
        public IChatClient Crear(string modelo)
        {
            var openAiKey = configuration.GetValue<string>("OpenAI_Key");
            var urlOllama = configuration.GetValue<string>("Ollama_Url");

            var proveedor = ModelosIA.ObtenerProveedor(modelo);

            var cliente = proveedor switch
            {
                "openai" => new OpenAI.Chat.ChatClient(modelo ?? "gpt-5.4-nano", openAiKey).AsIChatClient(),
                "ollama" => new OllamaApiClient(urlOllama, modelo ?? "qwen3.5:2b"),
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
