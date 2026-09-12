using CoreWCF;
using FacturacionApi.Models;

namespace FacturacionApi.Soap
{
    [ServiceContract(Name = "IFacturacionService", Namespace = "http://ecommerce.com/facturacion")]
    [XmlSerializerFormat]
    public interface IFacturacionService
    {
        [OperationContract]
        ComprobanteFiscal TimbrarComprobante(TimbradoRequest request);

        [OperationContract]
        ComprobanteFiscal ConsultarComprobante(string folioFiscal);
    }
}
