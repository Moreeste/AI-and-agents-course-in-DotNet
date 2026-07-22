using ChatBot;
using OpenAI.Chat;

Utilidades.CargarVariablesDeEntorno();

var modelo = "gpt-5.4-nano";
var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

var cliente = new ChatClient(modelo, key);

var respuesta = await cliente.CompleteChatAsync("Hola, ¿cómo estás?");

Console.WriteLine($"AI: {respuesta.Value.Content[0].Text}");