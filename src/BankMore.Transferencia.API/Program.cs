using BankMore.Infrastructure.Data;
using BankMore.Infrastructure.Services;
using BankMore.Shared.Interfaces;
using BankMore.Transferencia.API.Middleware; 
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BankMore Transferência API",
        Version = "v1",
        Description = "API especializada em transferências bancárias"
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Insira a API Key para acessar os endpoints de transferência:<br><strong>BankMore-Transfer-2024-Secure</strong>",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<BankMoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IHttpContaCorrenteService, HttpContaCorrenteService>(client =>
{
    // IMPORTANTE: Certifique-se que aponta para BankMore.API (5294)
    client.BaseAddress = new Uri(builder.Configuration["ContaCorrenteApi:BaseUrl"]
        ?? "http://localhost:5294/"); // ← Deve ser 5294 (BankMore.API)
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BankMoreDbContext>();
    context.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transferência API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();
app.Run();