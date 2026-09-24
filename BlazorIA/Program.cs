using BlazorIA.Components;
using BlazorIA.Datos;
using BlazorIA.RAG.ChatBots;
using BlazorIA.RAG.Servicios;
using BlazorIA.Servicios;
using BlazorIA.Servicios.ChatBots;
using BlazorIA.Utilidades;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<ApplicationDbContext>(opciones =>
    opciones.UseSqlite("Data Source=midb.db"));

builder.Services.AddScoped<IServicioPersonas, ServicioPersonas>();

builder.Services.AddKeyedScoped<IChatBot, ChatBotReal>("chat");
builder.Services.AddKeyedScoped<IChatBot, ChatBotRag>("chat-rag");

builder.Services.AddSingleton<ServicioDocumentosEnMemoria>();
builder.Services.AddSingleton<IServicioRag, ServicioRagMemoria>();
builder.Services.AddSingleton<InMemoryVectorStore>();

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apikey = configuration["OpenAI_Key"];
    var modeloEmbedding = "text-embedding-3-small";

    var cliente = new EmbeddingClient(modeloEmbedding, apikey);
    return cliente.AsIEmbeddingGenerator();
});

builder.Services.AddTransient<IServicioClima, ServicioClimaOpenWeather>();
builder.Services.AddTransient<ServicioEvaluaCondiciones>();
builder.Services.AddTransient<ServicioEnviarCorreoFalso>();
builder.Services.AddTransient<ServicioObtenerCorreoFalso>();
builder.Services.AddHttpClient();

builder.Services.AddTransient<IChatClientFactory, ChatClientFactory>();

builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions
{
    Tools = [.. Tools.ObtenerTools(sp)],
    Temperature = 0.7f,
    MaxOutputTokens = 2000
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
