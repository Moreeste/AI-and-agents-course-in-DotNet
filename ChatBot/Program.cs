using ChatBot;
using ChatBot.ChatBots;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Utilidades.CargarVariablesDeEntorno();

var proveedor = args.Length > 0 ? args[0].ToLowerInvariant() : "openai";
var modeloPorDefecto = proveedor == "openai" ? "gpt-5.4-nano" : "claude-haiku-4.5";
var modelo = args.Length > 1 ? args[1] : modeloPorDefecto;

Console.WriteLine($"{proveedor}:{modelo}");

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, proveedor, modelo);
var host = builder.Build();

var chatClient = host.Services.GetRequiredService<IChatClient>();
await ChatBotBase.Run(chatClient);