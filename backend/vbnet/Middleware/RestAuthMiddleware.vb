Imports System
Imports System.Text.Json
Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Http

Namespace Middleware
    Public Class RestAuthMiddleware
        Private ReadOnly _next As RequestDelegate
        Private Const ValidBearer As String = "Bearer test_token_2026"
        Private Const ValidApiKey As String = "secret_key_cs"

        Public Sub New([next] As RequestDelegate)
            _next = [next]
        End Sub

        Public Async Function InvokeAsync(context As HttpContext) As Task
            Dim path = If(context.Request.Path.Value, "")

            ' Omitir autenticación para preflight OPTIONS, swagger, wsdl y health check
            If context.Request.Method = "OPTIONS" OrElse Not path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) Then
                Await _next(context)
                Return
            End If

            Dim authHeader = context.Request.Headers("Authorization").ToString()
            Dim apiKeyHeader = context.Request.Headers("X-API-Key").ToString()

            Dim authorized As Boolean = (authHeader = ValidBearer) OrElse (apiKeyHeader = ValidApiKey)

            If Not authorized Then
                context.Response.StatusCode = StatusCodes.Status401Unauthorized
                context.Response.ContentType = "application/json"

                Dim err = New With {
                    Key .[error] = "Unauthorized",
                    Key .message = "Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'",
                    Key .statusCode = 401,
                    Key .timestamp = DateTime.UtcNow.ToString("o")
                }

                Await context.Response.WriteAsync(JsonSerializer.Serialize(err))
                Return
            End If

            Await _next(context)
        End Function
    End Class
End Namespace
