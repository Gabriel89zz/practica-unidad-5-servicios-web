using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using FacturacionApi.Middleware;
using FacturacionApi.Services;
using FacturacionApi.Soap;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar puerto 8085
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8085);
});

// Configuración de servicios REST
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Microservicio de Facturación y Timbrado Fiscal (.NET C#)",
        Version = "v1",
        Description = "Módulo empresarial para historial de facturas (REST) y timbrado transaccional de comprobantes fiscales SAT (SOAP CoreWCF). Práctica de Programación en Ambiente Cliente-Servidor."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorización JWT. Ejemplo: \"Bearer test_token_2026\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Encabezado de API-Key. Ejemplo: \"secret_key_cs\"",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configuración de CoreWCF (SOAP)
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

// Almacenamiento e inyección de dependencias
builder.Services.AddSingleton<FacturaStorage>();
builder.Services.AddTransient<FacturacionService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Facturacion API v1");
    c.RoutePrefix = "swagger";
});

// Middleware de Autenticación REST
app.UseMiddleware<RestAuthMiddleware>();

// CoreWCF Middleware
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<FacturacionService>(serviceOptions => { })
                  .AddServiceEndpoint<FacturacionService, IFacturacionService>(
                      new BasicHttpBinding(),
                      "/soap/FacturacionService.svc"
                  );

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
    serviceMetadataBehavior.HttpGetUrl = new Uri("/soap/FacturacionService.svc", UriKind.Relative);
});

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    servicio = "Microservicio de Facturacion y Timbrado Fiscal (C# .NET)",
    version = "1.0.0",
    puerto = 8085,
    swagger = "/swagger",
    soap_wsdl = "/soap/FacturacionService.svc?wsdl"
}));

app.Run();
