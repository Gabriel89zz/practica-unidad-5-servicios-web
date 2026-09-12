Imports System
Imports CoreWCF
Imports AuditoriaApi.Services

Namespace Soap
    Public Class AuditoriaService
        Implements IAuditoriaService

        Private ReadOnly _storage As AuditoriaStorage

        Public Sub New(storage As AuditoriaStorage)
            _storage = storage
        End Sub

        Public Function RegistrarEvento(request As EventoAuditoriaRequest) As RespuestaAuditoria Implements IAuditoriaService.RegistrarEvento
            ' Validación de credenciales si se suministran
            If Not String.IsNullOrEmpty(request.UsuarioAuth) Then
                If request.UsuarioAuth <> "admin" OrElse request.PasswordAuth <> "admin_pass_2026" Then
                    Throw New FaultException("Credenciales de autenticación SOAP inválidas para el módulo de auditoría.", New FaultCode("Client.AuthenticationFailed"))
                End If
            End If

            If String.IsNullOrWhiteSpace(request.ServicioOrigen) OrElse String.IsNullOrWhiteSpace(request.Accion) Then
                Throw New FaultException("El servicioOrigen y la acción son campos obligatorios para registrar un evento.", New FaultCode("Client.InvalidData"))
            End If

            Dim ev = _storage.Registrar(request.ServicioOrigen, request.Accion, request.Usuario, request.Detalles, request.Nivel)

            Dim resp As New RespuestaAuditoria With {
                .EventoId = ev.Id,
                .RegistradoExitosamente = True,
                .Timestamp = ev.Timestamp.ToString("o"),
                .Mensaje = "Evento registrado atómicamente en la bitácora del sistema."
            }
            Return resp
        End Function

        Public Function ConsultarTotalEventos() As Integer Implements IAuditoriaService.ConsultarTotalEventos
            Return _storage.TotalEventos()
        End Function
    End Class
End Namespace
