Imports System.Runtime.Serialization
Imports CoreWCF

Namespace Soap
    <DataContract(Namespace:="http://ecommerce.com/auditoria")>
    Public Class EventoAuditoriaRequest
        <DataMember>
        Public Property ServicioOrigen As String = String.Empty

        <DataMember>
        Public Property Accion As String = String.Empty

        <DataMember>
        Public Property Usuario As String = String.Empty

        <DataMember>
        Public Property Detalles As String = String.Empty

        <DataMember>
        Public Property Nivel As String = "INFO"

        <DataMember>
        Public Property UsuarioAuth As String

        <DataMember>
        Public Property PasswordAuth As String
    End Class

    <DataContract(Namespace:="http://ecommerce.com/auditoria")>
    Public Class RespuestaAuditoria
        <DataMember>
        Public Property EventoId As String = String.Empty

        <DataMember>
        Public Property RegistradoExitosamente As Boolean

        <DataMember>
        Public Property Timestamp As String = String.Empty

        <DataMember>
        Public Property Mensaje As String = String.Empty
    End Class

    <ServiceContract(Name:="IAuditoriaService", Namespace:="http://ecommerce.com/auditoria")>
    <XmlSerializerFormat>
    Public Interface IAuditoriaService
        <OperationContract>
        Function RegistrarEvento(request As EventoAuditoriaRequest) As RespuestaAuditoria

        <OperationContract>
        Function ConsultarTotalEventos() As Integer
    End Interface
End Namespace
