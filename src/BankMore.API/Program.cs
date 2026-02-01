using BankMore.API.Data;
using BankMore.API.Services;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using BankMore.Infrastructure.Migrations;
using BankMore.Infrastructure.Services;
using BankMore.Shared.Interfaces;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BankMore API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<BankMoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret não configurada"));

builder.Services.AddHttpClient<IHttpContaCorrenteService, HttpContaCorrenteService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ContaCorrenteApi:BaseUrl"]
        ?? "http://localhost:5001/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<BankMore.API.Services.IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IMovimentacaoService, MovimentacaoService>();
builder.Services.AddScoped<ISaldoService, SaldoService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<BankMore.API.Services.ITransferenciaService, BankMore.API.Services.TransferenciaService>();
builder.Services.AddScoped<TransferenciaOrquestradorService>();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSQLite()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ScanIn(typeof(InitialDatabase).Assembly).For.Migrations()
        .ScanIn(typeof(InitialDatabase).Assembly).For.EmbeddedResources())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await SeedData.Initialize(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankMore API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🔄 Configurando banco de dados...");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BankMoreDbContext>();

        var sql = @"
            CREATE TABLE IF NOT EXISTS VersionInfo (
                Version BIGINT NOT NULL PRIMARY KEY,
                AppliedOn DATETIME,
                Description TEXT
            );
            
            CREATE TABLE IF NOT EXISTS ContasCorrentes (
                Id TEXT PRIMARY KEY,
                NumeroConta TEXT NOT NULL UNIQUE,
                CPFCriptografado TEXT NOT NULL,
                CPFHash TEXT NOT NULL,
                SenhaHash TEXT NOT NULL,
                NomeTitular TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                Ativa BOOLEAN NOT NULL DEFAULT 1,
                DataCriacao DATETIME NOT NULL,
                DataInativacao DATETIME
            );
            
            CREATE TABLE IF NOT EXISTS Movimentacoes (
                Id TEXT PRIMARY KEY,
                ContaId TEXT NOT NULL,
                Tipo TEXT NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                Descricao TEXT,
                DataMovimentacao DATETIME NOT NULL,
                ContaDestino TEXT,
                IdRequisicao TEXT,  
                FOREIGN KEY (ContaId) REFERENCES ContasCorrentes(Id)
            );
            
            CREATE TABLE IF NOT EXISTS Transferencias (
                Id TEXT PRIMARY KEY,
                ContaOrigemId TEXT NOT NULL,
                ContaDestinoId TEXT NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                DataTransferencia DATETIME NOT NULL,
                Status TEXT NOT NULL,
                CodigoRastreio TEXT,
                FOREIGN KEY (ContaOrigemId) REFERENCES ContasCorrentes(Id),
                FOREIGN KEY (ContaDestinoId) REFERENCES ContasCorrentes(Id)
            );
            
            CREATE TABLE IF NOT EXISTS Tarifas (
                Id TEXT PRIMARY KEY,
                Tipo TEXT NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                Descricao TEXT,
                Ativa BOOLEAN NOT NULL DEFAULT 1,
                DataCriacao DATETIME NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS IdempotencyKeys (
                Id TEXT PRIMARY KEY,
                RequestType TEXT NOT NULL,
                ContaOrigem TEXT,
                ContaDestino TEXT,
                Valor DECIMAL(18,2),
                Status TEXT NOT NULL,
                ResponseData TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );
            
            INSERT OR IGNORE INTO VersionInfo (Version, AppliedOn, Description) 
            VALUES (202412150000, datetime('now'), 'InitialDatabase'),
                   (202412161000, datetime('now'), 'AddIdempotencyKeysTable');
        ";

        dbContext.Database.ExecuteSqlRaw(sql);
        Console.WriteLine("✅ Todas as tabelas criadas/verificadas");

        if (!dbContext.ContasCorrentes.Any())
        {
            var hasher = new PasswordHasher();
            var crypto = new CryptoService();
            var contaTeste = new ContaCorrente
            {
                Id = Guid.NewGuid(),
                CPFCriptografado = crypto.Criptografar("72057376052"),
                CPFHash = crypto.Hash("72057376052"),
                NomeTitular = "Antonio Bruno",
                Email = "abruno@bankmore.com",
                SenhaHash = hasher.HashPassword("123456"),
                NumeroConta = "000001",
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            dbContext.ContasCorrentes.Add(contaTeste);
            dbContext.SaveChanges();
            Console.WriteLine("👤 Usuário teste criado: 000001 / 123456");
        }

        Console.WriteLine("🎉 Banco configurado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERRO CRÍTICO no banco: {ex.Message}");
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();