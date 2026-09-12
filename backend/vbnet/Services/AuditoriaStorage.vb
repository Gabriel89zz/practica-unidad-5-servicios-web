Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports AuditoriaApi.Models

Namespace Services
    Public Class AuditoriaStorage
        Private ReadOnly _logs As New ConcurrentDictionary(Of String, LogEvento)()

        Public Sub New()
            SeedData()
        End Sub

        Private Sub SeedData()
            Dim e1 As New LogEvento With {
                .Id = "LOG-2026-0001",
                .ServicioOrigen = "MODULO-INVENTARIO",
                .Accion = "ACTUALIZAR_STOCK",
                .Usuario = "sistema_admin",
                .Nivel = "INFO",
                .Detalles = "Stock actualizado para producto LAP-001 (+15 unidades)",
                .IpCliente = "127.0.0.1",
                .Timestamp = DateTime.UtcNow.AddMinutes(-45)
            }

            Dim e2 As New LogEvento With {
                .Id = "LOG-2026-0002",
                .ServicioOrigen = "MODULO-ORDENES",
                .Accion = "CREAR_ORDEN",
                .Usuario = "cliente_web",
                .Nivel = "INFO",
                .Detalles = "Orden ORD-2026-101 generada por monto total $2,203.98",
                .IpCliente = "192.168.1.100",
                .Timestamp = DateTime.UtcNow.AddMinutes(-30)
            }

            Dim e3 As New LogEvento With {
                .Id = "LOG-2026-0003",
                .ServicioOrigen = "MODULO-FACTURACION",
                .Accion = "TIMBRAR_CFDI",
                .Usuario = "sat_gateway",
                .Nivel = "INFO",
                .Detalles = "Timbrado exitoso para RFC GOLF850315ABC UUID A1B2C3D4-...",
                .IpCliente = "127.0.0.1",
                .Timestamp = DateTime.UtcNow.AddMinutes(-20)
            }

            _logs(e1.Id) = e1
            _logs(e2.Id) = e2
            _logs(e3.Id) = e3
        End Sub

        Public Function Listar(servicio As String, nivel As String) As IEnumerable(Of LogEvento)
            Dim q = _logs.Values.AsEnumerable()

            If Not String.IsNullOrWhiteSpace(servicio) Then
                q = q.Where(Function(l) l.ServicioOrigen.IndexOf(servicio, StringComparison.OrdinalIgnoreCase) >= 0)
            End If

            If Not String.IsNullOrWhiteSpace(nivel) Then
                q = q.Where(Function(l) l.Nivel.Equals(nivel, StringComparison.OrdinalIgnoreCase))
            End If

            Return q.OrderByDescending(Function(l) l.Timestamp).ToList()
        End Function

        Public Function ObtenerPorId(id As String) As LogEvento
            Dim l As LogEvento = Nothing
            _logs.TryGetValue(id.Trim().ToUpper(), l)
            Return l
        End Function

        Public Function Registrar(servicio As String, accion As String, usuario As String, detalles As String, nivel As String) As LogEvento
            Dim id = "LOG-2026-" & Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()
            Dim nuevo As New LogEvento With {
                .Id = id,
                .ServicioOrigen = If(String.IsNullOrWhiteSpace(servicio), "GENERAL", servicio.ToUpper()),
                .Accion = If(String.IsNullOrWhiteSpace(accion), "EVENTO", accion.ToUpper()),
                .Usuario = If(String.IsNullOrWhiteSpace(usuario), "anonimo", usuario),
                .Detalles = detalles,
                .Nivel = If(String.IsNullOrWhiteSpace(nivel), "INFO", nivel.ToUpper()),
                .IpCliente = "127.0.0.1",
                .Timestamp = DateTime.UtcNow
            }

            _logs(id) = nuevo
            Return nuevo
        End Function

        Public Function Purgar() As Integer
            Dim conteo = _logs.Count
            _logs.Clear()
            Return conteo
        End Function

        Public Function TotalEventos() As Integer
            Return _logs.Count
        End Function
    End Class
End Namespace
