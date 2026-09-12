Imports System

Namespace Models
    Public Class LogEvento
        Public Property Id As String = String.Empty
        Public Property ServicioOrigen As String = String.Empty
        Public Property Accion As String = String.Empty
        Public Property Usuario As String = String.Empty
        Public Property Nivel As String = "INFO" ' INFO, WARNING, ERROR, CRITICAL
        Public Property Detalles As String = String.Empty
        Public Property IpCliente As String = "127.0.0.1"
        Public Property Timestamp As DateTime = DateTime.UtcNow
    End Class

    Public Class RegistrarLogDto
        Public Property ServicioOrigen As String = String.Empty
        Public Property Accion As String = String.Empty
        Public Property Usuario As String = String.Empty
        Public Property Nivel As String = "INFO"
        Public Property Detalles As String = String.Empty
    End Class
End Namespace
