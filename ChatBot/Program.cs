using ChatBot;
using ChatBot.ChatBots;
using Microsoft.Extensions.AI;

Utilidades.CargarVariablesDeEntorno();

var modelo = "gpt-5.4-nano";
var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var cliente = new OpenAI.Chat.ChatClient(modelo, key).AsIChatClient();

await ChatBotBase.Run(cliente);