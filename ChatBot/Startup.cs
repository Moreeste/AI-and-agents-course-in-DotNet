using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatBot
{
    public class Startup
    {
        public static void ConfigureServices(HostApplicationBuilder builder, string proveedor, string? modelo)
        {
            string openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var cliente = proveedor switch
                {
                    "openai" => new OpenAI.Chat.ChatClient(modelo ?? "gpt-5.4-nano", openAiKey).AsIChatClient(),
                    _ => throw new ArgumentException($"Proveedor desconocido: {proveedor}"),
                };

                return cliente.AsBuilder()
                .Use(async (mensajes, opciones, next, cancellationToken) =>
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Antes de llamar al modelo...");
                    Console.ResetColor();

                    await next(mensajes, opciones, cancellationToken);

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Después de llamar al modelo...");
                    Console.ResetColor();

                }).Build(sp);
            });
        }
    }
}
