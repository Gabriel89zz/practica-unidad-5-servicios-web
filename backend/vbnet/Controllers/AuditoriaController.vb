Imports System
Imports System.Collections.Generic
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc
Imports AuditoriaApi.Models
Imports AuditoriaApi.Services

Namespace Controllers
    <ApiController>
    <Route("api/v1/logs")>
    <Produces("application/json")>
    Public Class AuditoriaController
        Inherits ControllerBase

        Private ReadOnly _storage As AuditoriaStorage

        Public Sub New(storage As AuditoriaStorage)
            _storage = storage
        End Sub

        <HttpGet>
        <ProducesResponseType(GetType(IEnumerable(Of LogEvento)), StatusCodes.Status200OK)>
        <ProducesResponseType(StatusCodes.Status401Unauthorized)>
        Public Function ListarLogs(<FromQuery> servicio As String, <FromQuery> nivel As String) As IActionResult
            Dim logs = _storage.Listar(servicio, nivel)
            Return Ok(logs)
        End Function

        <HttpGet("{id}")>
        <ProducesResponseType(GetType(LogEvento), StatusCodes.Status200OK)>
        <ProducesResponseType(StatusCodes.Status404NotFound)>
        <ProducesResponseType(StatusCodes.Status401Unauthorized)>
        Public Function ObtenerPorId(id As String) As IActionResult
            Dim l = _storage.ObtenerPorId(id)
            If l Is Nothing Then
                Return NotFound(New With {
                    Key .[error] = "Not Found",
                    Key .message = $"Registro de auditoría con ID '{id}' no encontrado.",
                    Key .statusCode = 404
                })
            End If
            Return Ok(l)
        End Function

        <HttpPost>
        <ProducesResponseType(GetType(LogEvento), StatusCodes.Status201Created)>
        <ProducesResponseType(StatusCodes.Status400BadRequest)>
        <ProducesResponseType(StatusCodes.Status401Unauthorized)>
        Public Function RegistrarLog(<FromBody> dto As RegistrarLogDto) As IActionResult
            If String.IsNullOrWhiteSpace(dto.ServicioOrigen) OrElse String.IsNullOrWhiteSpace(dto.Accion) Then
                Return BadRequest(New With {
                    Key .[error] = "Bad Request",
                    Key .message = "El servicioOrigen y la accion son campos obligatorios.",
                    Key .statusCode = 400
                })
            End If

            Dim nuevo = _storage.Registrar(dto.ServicioOrigen, dto.Accion, dto.Usuario, dto.Detalles, dto.Nivel)
            Return CreatedAtAction(NameOf(ObtenerPorId), New With {Key .id = nuevo.Id}, nuevo)
        End Function

        <HttpDelete>
        <ProducesResponseType(StatusCodes.Status200OK)>
        <ProducesResponseType(StatusCodes.Status401Unauthorized)>
        Public Function PurgarLogs() As IActionResult
            Dim totalPurgados = _storage.Purgar()
            Return Ok(New With {
                Key .message = $"{totalPurgados} registros de auditoría purgados del sistema.",
                Key .statusCode = 200
            })
        End Function
    End Class
End Namespace
