using System.Collections.Concurrent;
using FacturacionApi.Models;

namespace FacturacionApi.Services
{
    public class FacturaStorage
    {
        private readonly ConcurrentDictionary<string, FacturaModel> _facturas = new();
        private readonly ConcurrentDictionary<string, ComprobanteFiscal> _comprobantes = new();

        public FacturaStorage()
        {
            SeedData();
        }

        private void SeedData()
        {
            var f1 = new FacturaModel
            {
                Folio = "FAC-2026-001",
                FolioFiscalUUID = "A1B2C3D4-E5F6-7A8B-9C0D-1E2F3A4B5C6D",
                RfcCliente = "GOLF850315ABC",
                RazonSocialCliente = "Laura Gómez Flores",
                Subtotal = 1899.99m,
                Impuestos = 303.99m,
                Total = 2203.98m,
                Concepto = "Venta de equipo de cómputo y periféricos",
                Estatus = "VIGENTE",
                FechaEmision = DateTime.UtcNow.AddDays(-2)
            };

            var f2 = new FacturaModel
            {
                Folio = "FAC-2026-002",
                FolioFiscalUUID = "B2C3D4E5-F6A7-8B9C-0D1E-2F3A4B5C6D7E",
                RfcCliente = "MORR900101XYZ",
                RazonSocialCliente = "Roberto Morales",
                Subtotal = 229.49m,
                Impuestos = 36.71m,
                Total = 266.20m,
                Concepto = "Venta de periféricos inalámbricos",
                Estatus = "VIGENTE",
                FechaEmision = DateTime.UtcNow.AddDays(-1)
            };

            _facturas[f1.Folio] = f1;
            _facturas[f2.Folio] = f2;

            _comprobantes[f1.FolioFiscalUUID] = new ComprobanteFiscal
            {
                FolioFiscalUUID = f1.FolioFiscalUUID,
                RfcEmisor = "ECO20260101ECO",
                RfcReceptor = f1.RfcCliente,
                Total = f1.Total,
                FechaTimbrado = f1.FechaEmision.ToString("o"),
                SelloDigitalSAT = "SAT998877665544332211AABBCCDDEEFF001122334455",
                SelloDigitalEmisor = "EMI112233445566778899FFEEDDCCBBAA998877665544",
                Estatus = "TIMBRADO"
            };
        }

        public IEnumerable<FacturaModel> Listar(string? rfc, string? estatus)
        {
            var q = _facturas.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(rfc))
                q = q.Where(f => f.RfcCliente.Contains(rfc, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(estatus))
                q = q.Where(f => f.Estatus.Equals(estatus, StringComparison.OrdinalIgnoreCase));
            return q.OrderByDescending(f => f.FechaEmision);
        }

        public FacturaModel? ObtenerPorFolio(string folio)
        {
            _facturas.TryGetValue(folio.Trim().ToUpper(), out var f);
            return f;
        }

        public FacturaModel CrearFactura(CrearFacturaDto dto)
        {
            var folio = $"FAC-2026-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            var uuid = Guid.NewGuid().ToString().ToUpper();
            var fac = new FacturaModel
            {
                Folio = folio,
                FolioFiscalUUID = uuid,
                RfcCliente = dto.RfcCliente.ToUpper(),
                RazonSocialCliente = dto.RazonSocialCliente,
                Subtotal = dto.Subtotal,
                Impuestos = dto.Impuestos,
                Total = dto.Total,
                Concepto = dto.Concepto,
                Estatus = "VIGENTE",
                FechaEmision = DateTime.UtcNow,
                MetodoPago = dto.MetodoPago
            };
            _facturas[folio] = fac;
            return fac;
        }

        public ComprobanteFiscal Timbrar(TimbradoRequest req)
        {
            var uuid = Guid.NewGuid().ToString().ToUpper();
            var comp = new ComprobanteFiscal
            {
                FolioFiscalUUID = uuid,
                RfcEmisor = string.IsNullOrWhiteSpace(req.RfcEmisor) ? "ECO20260101ECO" : req.RfcEmisor.ToUpper(),
                RfcReceptor = req.RfcReceptor.ToUpper(),
                Total = req.Total,
                FechaTimbrado = DateTime.UtcNow.ToString("o"),
                SelloDigitalSAT = Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray()),
                SelloDigitalEmisor = Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray()),
                Estatus = "TIMBRADO",
                CodigoRespuesta = "SAT-000-OK",
                Mensaje = $"Comprobante fiscal timbrado con éxito bajo folio {uuid}"
            };
            _comprobantes[uuid] = comp;

            // También guardar en el registro de facturas
            var folio = $"FAC-2026-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            _facturas[folio] = new FacturaModel
            {
                Folio = folio,
                FolioFiscalUUID = uuid,
                RfcCliente = req.RfcReceptor.ToUpper(),
                RazonSocialCliente = "Cliente Receptor SAT",
                Subtotal = req.Subtotal,
                Impuestos = req.Iva,
                Total = req.Total,
                Concepto = req.Concepto,
                Estatus = "TIMBRADA",
                FechaEmision = DateTime.UtcNow
            };

            return comp;
        }

        public ComprobanteFiscal? ObtenerComprobante(string uuid)
        {
            _comprobantes.TryGetValue(uuid.Trim().ToUpper(), out var c);
            return c;
        }
    }
}
