var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer().WithHttpTransport();

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
