using ServidorMCP.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepositorioPersonas, RepositorioPersonasMemoria>();

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapMcp("/mcp");

app.Run();
