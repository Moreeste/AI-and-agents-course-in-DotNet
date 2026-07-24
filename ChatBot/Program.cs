using ChatBot;
using OpenAI.Chat;
using System.Text;

Utilidades.CargarVariablesDeEntorno();

var modelo = "gpt-5.4-nano";
var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var cliente = new ChatClient(modelo, key);

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

mensajes.Add(new SystemChatMessage(systemPromptCSharp));

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

    mensajes.Add(new UserChatMessage(entrada));

    Console.WriteLine();
    Console.Write($"AI: ");

    var stream = cliente.CompleteChatStreamingAsync(mensajes);

    await foreach (var actualizacion in stream)
    {
        foreach (var contenido in actualizacion.ContentUpdate)
        {
            sb.Append(contenido.Text);
            Console.Write(contenido.Text);
        }
    }

    mensajes.Add(new AssistantChatMessage(sb.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}