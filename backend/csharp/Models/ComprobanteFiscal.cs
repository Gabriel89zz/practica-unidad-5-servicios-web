using System.Runtime.Serialization;

namespace FacturacionApi.Models
{
    [DataContract(Namespace = "http://ecommerce.com/facturacion")]
    public class TimbradoRequest
    {
        [DataMember]
        public string RfcEmisor { get; set; } = string.Empty;

        [DataMember]
        public string RfcReceptor { get; set; } = string.Empty;

        [DataMember]
        public decimal Subtotal { get; set; }

        [DataMember]
        public decimal Iva { get; set; }

        [DataMember]
        public decimal Total { get; set; }

        [DataMember]
        public string Concepto { get; set; } = string.Empty;

        [DataMember]
        public string FormaPago { get; set; } = "03"; // Transferencia / Tarjeta

        [DataMember]
        public string? UsuarioAuth { get; set; }

        [DataMember]
        public string? PasswordAuth { get; set; }
    }

    [DataContract(Namespace = "http://ecommerce.com/facturacion")]
    public class ComprobanteFiscal
    {
        [DataMember]
        public string FolioFiscalUUID { get; set; } = string.Empty;

        [DataMember]
        public string RfcEmisor { get; set; } = string.Empty;

        [DataMember]
        public string RfcReceptor { get; set; } = string.Empty;

        [DataMember]
        public decimal Total { get; set; }

        [DataMember]
        public string FechaTimbrado { get; set; } = string.Empty;

        [DataMember]
        public string SelloDigitalSAT { get; set; } = string.Empty;

        [DataMember]
        public string SelloDigitalEmisor { get; set; } = string.Empty;

        [DataMember]
        public string NoCertificadoSAT { get; set; } = "30001000000500003416";

        [DataMember]
        public string Estatus { get; set; } = "TIMBRADO";

        [DataMember]
        public string CodigoRespuesta { get; set; } = "SAT-000-OK";

        [DataMember]
        public string Mensaje { get; set; } = "Comprobante fiscal timbrado exitosamente";
    }

    public class FacturaModel
    {
        public string Folio { get; set; } = string.Empty;
        public string FolioFiscalUUID { get; set; } = string.Empty;
        public string RfcCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Estatus { get; set; } = "VIGENTE";
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
        public string MetodoPago { get; set; } = "PUE";
    }

    public class CrearFacturaDto
    {
        public string RfcCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = "PUE";
    }

    public class ActualizarFacturaDto
    {
        public string? Concepto { get; set; }
        public string? Estatus { get; set; }
    }
}

