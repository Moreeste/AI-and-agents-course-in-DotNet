using BlazorIA.Components;
using BlazorIA.Servicios.ChatBots;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IChatBot, ChatBotReal>();

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var proveedor = "openai";
    var modelo = "gpt-5.4-nano";
    var openAiKey = configuration.GetValue<string>("OpenAI_Key");

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
    })
    .UseFunctionInvocation(null, c =>
    {
        c.IncludeDetailedErrors = true;
    })
    .Build(sp);
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
