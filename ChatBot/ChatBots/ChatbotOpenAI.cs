using Microsoft.Extensions.AI;
using System.Text;

namespace ChatBot.ChatBots
{
    public class ChatbotOpenAI
    {
        public static async Task Run()
        {

            var modelo = "gpt-5.4-nano";
            var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var cliente = new OpenAI.Chat.ChatClient(modelo, key).AsIChatClient();

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
                var sb = new StringBuilder();
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

                await foreach (var fragmento in cliente.GetStreamingResponseAsync(mensajes))
                {
                    sb.Append(fragmento);
                    Console.Write(fragmento);
                }

                mensajes.Add(new ChatMessage(role: ChatRole.Assistant, sb.ToString()));

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
