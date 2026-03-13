using System.Reflection;
using AirWatch.Api.Middleware;
using AirWatch.Application.Services;
using AirWatch.Infrastructure;
using AirWatch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

            Sensores da série MQ conectados a um ESP8266 enviam dados ambientais (gás, temperatura e umidade)
            via MQTT. Esta API recebe, armazena e disponibiliza essas medições para visualização no frontend.

            ## Recursos disponíveis

            - **Sensores** — cadastro e consulta dos dispositivos registrados no sistema
            - **Medições** — registro e consulta dos dados coletados pelos sensores
            - **Usuários** — cadastro de usuários do sistema (autenticação será implementada futuramente)

            ## Códigos de resposta

            | Código | Significado |
            |--------|-------------|
            | 200 | Requisição bem-sucedida |
            | 201 | Recurso criado com sucesso |
            | 400 | Dados inválidos na requisição |
            | 404 | Recurso não encontrado |
            | 409 | Conflito — recurso já existe |
            | 500 | Erro interno do servidor |
            """
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<MeasurementService>();
builder.Services.AddScoped<SensorService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

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
app.UseAuthorization();
app.MapControllers();

app.Run();
