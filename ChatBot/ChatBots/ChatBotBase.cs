using Microsoft.Extensions.AI;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChatBot.ChatBots
{
    public class ChatBotBase
    {
        public static async Task Run(IChatClient cliente)
        {
            Console.WriteLine("AI: ¡Hola! Puedes escribir tus preguntas o presionar Enter para salir.");
            Console.WriteLine();

            var mensajes = new List<ChatMessage>();

            var systemPromptGeneral = """
            Eres un asistente que responde preguntas generales.
            Debes responder en español.
            Las respuestas deben ser en texto plano, no usar formatos como markdown.
            """;

            var systemPromptCSharp = """
            Eres un asistente experto en c# y .NET.
            Debes responder en español y dando ejemplos.
            Las respuestas deben ser en texto plano, no usar formatos como markdown.
            """;

            var systemPromptPython = """
            Eres un asistente experto en Python.
            Debes responder en español y dando ejemplos.
            Las respuestas deben ser en texto plano, no usar formatos como markdown.
            """;

            mensajes.Add(new ChatMessage(role: ChatRole.System, systemPromptCSharp));

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Tú: ");
                var entrada = Console.ReadLine();
                Console.ResetColor();

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    break;
                }

                mensajes.Add(new ChatMessage(role: ChatRole.User, entrada));

                Console.WriteLine();
                Console.Write($"AI: ");

                while (true)
                {
                    var updates = new List<ChatResponseUpdate>();

                    await foreach (var responseUpdate in cliente.GetStreamingResponseAsync(mensajes))
                    {
                        updates.Add(responseUpdate);

                        foreach (var contenido in responseUpdate.Contents)
                        {
                            if (contenido is TextContent contenidoTexto)
                            {
                                Console.Write(contenidoTexto);
                            }
                        }
                    }

                    var respuesta = updates.ToChatResponse();
                    mensajes.AddMessages(respuesta);

                    var solicitudAprobacion = respuesta.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().FirstOrDefault();

                    if (solicitudAprobacion is not null)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("La IA desea ejecutar una acción sensible.");

                        if (solicitudAprobacion.ToolCall is FunctionCallContent functionCall)
                        {
                            Console.WriteLine($"Tool: {ConvertirNombreDeFuncion(functionCall.Name)}");

                            if (functionCall.Arguments is not null)
                            {
                                foreach (var argumento in functionCall.Arguments)
                                {
                                    Console.WriteLine($"{argumento.Key}: {argumento.Value}");
                                }
                            }
                        }

                        Console.ResetColor();
                        Console.Write("¿Deseas aprobar esta acción? (s/n): ");
                        var aprobada = Console.ReadLine()?.Trim().ToLower() == "s";
                        var respuestaAprobacion = solicitudAprobacion.CreateResponse(aprobada);

                        mensajes.Add(new ChatMessage(ChatRole.User, [respuestaAprobacion]));

                        Console.WriteLine();
                        Console.Write("IA: ");
                        continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    break;
                }
            }
        }

        private static string ConvertirNombreDeFuncion(string nombre)
        {
            return nombre switch
            {
                "EnviarCorreo" => "Enviar correo",
                _ => nombre
            };
        }
    }
}
