using System.Reflection;
using System.Text;
using AirWatch.Api.Middleware;
using AirWatch.Application.Services;
using AirWatch.Infrastructure;
using AirWatch.Infrastructure.BackgroundServices;
using AirWatch.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AirWatch API",
        Version = "v1",
        Description = """
            API do sistema **AirWatch** — monitoramento da qualidade do ar via IoT.

            Sensores MQ3, MQ5 e MQ135 conectados a um Arduino com placa ESP enviam leituras
            via MQTT. Esta API recebe, armazena e disponibiliza as medições para o frontend Angular.

            ## Como autenticar

            1. Cadastre um usuário em `POST /api/users`
            2. Faça login em `POST /api/auth/login` e copie o `token` retornado
            3. Clique no botão **Authorize** acima e cole o token no campo `Bearer {token}`

            ## Recursos disponíveis

            - **Auth** — login e geração de token JWT
            - **Dispositivos** — cadastro e consulta dos Arduinos registrados no sistema
            - **Medições** — consulta dos dados coletados pelos sensores MQ (ppm, tensão, Rs/R0)
            - **Usuários** — cadastro de usuários do sistema

            ## Códigos de resposta

            | Código | Significado |
            |--------|-------------|
            | 200 | Requisição bem-sucedida |
            | 201 | Recurso criado com sucesso |
            | 400 | Dados inválidos na requisição |
            | 401 | Não autenticado — token ausente ou inválido |
            | 404 | Recurso não encontrado |
            | 409 | Conflito — recurso já existe |
            | 500 | Erro interno do servidor |
            """
    });

    // Botão de autenticação no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido em POST /api/auth/login.\n\nExemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MeasurementService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<UserService>();

// Simulador MQTT (ativado via appsettings)
if (builder.Configuration.GetValue<bool>("Simulator:Enabled"))
    builder.Services.AddHostedService<MqttSimulatorService>();

// Assinante MQTT real (ativado via appsettings)
if (builder.Configuration.GetValue<bool>("MqttSubscriber:Enabled"))
    builder.Services.AddHostedService<MqttSubscriberService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Health Check
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!,
               name: "postgresql",
               tags: ["db", "ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AirWatch API v1");
        options.DocumentTitle = "AirWatch — Documentação da API";
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
