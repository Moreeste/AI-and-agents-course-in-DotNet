using ChatBot.Servicios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatBot
{
    public class Startup
    {
        public static void ConfigureServices(HostApplicationBuilder builder, string proveedor, string? modelo)
        {
            string openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            builder.Services.AddSingleton<IServicioClima, ServicioClimaOpenWeather>();
            builder.Services.AddTransient<ServicioEvaluaCondiciones>();
            builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
            builder.Services.AddHttpClient();

            builder.Services.AddTransient<ServicioObtenerCorreoFalso>();
            builder.Services.AddTransient<ServicioEnviarCorreoFalso>();

            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var cliente = proveedor switch
                {
                    "openai" => new OpenAI.Chat.ChatClient(modelo ?? "gpt-5.4-nano", openAiKey).AsIChatClient(),
                    _ => throw new ArgumentException($"Proveedor desconocido: {proveedor}"),
                };

                return cliente.AsBuilder()
                .ConfigureOptions(o =>
                {
                    o.MaxOutputTokens = 2000;
                    o.Temperature = 0.7f;
                    o.Tools = [.. Tools.Tools.ObtenerTools(sp)];
                })
                .UseFunctionInvocation(null, c =>
                {
                    c.IncludeDetailedErrors = true;
                })
                .Use(async (messages, options, next, cancellationToken) =>
                {
                    await next(messages, options, cancellationToken);
                })
                .Build(sp);
            });
        }
    }
}
