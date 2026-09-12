Imports System
Imports CoreWCF
Imports CoreWCF.Configuration
Imports CoreWCF.Description
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.AspNetCore.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.OpenApi.Models
Imports AuditoriaApi.Middleware
Imports AuditoriaApi.Services
Imports AuditoriaApi.Soap

Namespace AuditoriaApi
    Module Program
        Sub Main(args As String())
            Dim builder = WebApplication.CreateBuilder(args)

            builder.WebHost.ConfigureKestrel(Sub(options)
                options.ListenAnyIP(8086)
            End Sub)

            ' Servicios REST
            builder.Services.AddControllers()
            builder.Services.AddEndpointsApiExplorer()
            builder.Services.AddSwaggerGen(Sub(c)
                c.SwaggerDoc("v1", New OpenApiInfo With {
                    .Title = "Microservicio de Auditoria y Bitacoras (VB.NET)",
                    .Version = "v1",
                    .Description = "Módulo empresarial de consulta de auditoría (REST) y registro atómico de eventos (SOAP CoreWCF). Práctica de Programación en Ambiente Cliente-Servidor."
                })

                c.AddSecurityDefinition("Bearer", New OpenApiSecurityScheme With {
                    .Description = "Encabezado de autorizacion JWT. Ejemplo: ""Bearer test_token_2026""",
                    .Name = "Authorization",
                    .In = ParameterLocation.Header,
                    .Type = SecuritySchemeType.ApiKey,
                    .Scheme = "Bearer"
                })

                c.AddSecurityDefinition("ApiKey", New OpenApiSecurityScheme With {
                    .Description = "Encabezado API-Key. Ejemplo: ""secret_key_cs""",
                    .Name = "X-API-Key",
                    .In = ParameterLocation.Header,
                    .Type = SecuritySchemeType.ApiKey
                })

                c.AddSecurityRequirement(New OpenApiSecurityRequirement From {
                    {
                        New OpenApiSecurityScheme With {
                            .Reference = New OpenApiReference With {.Type = ReferenceType.SecurityScheme, .Id = "Bearer"}
                        },
                        Array.Empty(Of String)()
                    }
                })
            End Sub)

            ' CoreWCF SOAP
            builder.Services.AddServiceModelServices()
            builder.Services.AddServiceModelMetadata()
            builder.Services.AddSingleton(Of IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior)()

            ' Almacenamiento e Inyeccion
            builder.Services.AddSingleton(Of AuditoriaStorage)()
            builder.Services.AddTransient(Of AuditoriaService)()

            ' CORS
            builder.Services.AddCors(Sub(options)
                options.AddDefaultPolicy(Sub(policy)
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
                End Sub)
            End Sub)

            Dim app = builder.Build()

            app.UseCors()

            app.UseSwagger()
            app.UseSwaggerUI(Sub(c)
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auditoria API v1")
                c.RoutePrefix = "swagger"
            End Sub)

            app.UseMiddleware(Of RestAuthMiddleware)()

            WcfHelper.SoapConfigurator.ConfigureServiceModel(app, Sub(serviceBuilder As IServiceBuilder)
                serviceBuilder.AddService(Of AuditoriaService)(Sub(serviceOptions)
                End Sub).AddServiceEndpoint(Of AuditoriaService, IAuditoriaService)(
                    New BasicHttpBinding(),
                    "/soap/AuditoriaService.svc"
                )

                Dim metadata = app.Services.GetRequiredService(Of ServiceMetadataBehavior)()
                metadata.HttpGetEnabled = True
                metadata.HttpGetUrl = New Uri("/soap/AuditoriaService.svc", UriKind.Relative)
            End Sub)

            app.MapControllers()

            app.MapGet("/", Function() Results.Ok(New With {
                Key .servicio = "Microservicio de Auditoria y Bitacoras (VB.NET)",
                Key .version = "1.0.0",
                Key .puerto = 8086,
                Key .swagger = "/swagger",
                Key .soap_wsdl = "/soap/AuditoriaService.svc?wsdl"
            }))

            app.Run()
        End Sub
    End Module
End Namespace
