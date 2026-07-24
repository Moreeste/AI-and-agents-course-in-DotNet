using ChatBot;
using OpenAI.Chat;

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

    var respuesta = await cliente.CompleteChatAsync(mensajes);
    var respuestaAI = respuesta.Value.Content[0].Text;
    mensajes.Add(new AssistantChatMessage(respuestaAI));

    Console.WriteLine($"AI: {respuestaAI}");
    Console.WriteLine();
}