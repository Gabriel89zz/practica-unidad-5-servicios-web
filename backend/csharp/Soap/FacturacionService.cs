using CoreWCF;
using FacturacionApi.Models;
using FacturacionApi.Services;

namespace FacturacionApi.Soap
{
    public class FacturacionService : IFacturacionService
    {
        private readonly FacturaStorage _storage;

        public FacturacionService(FacturaStorage storage)
        {
            _storage = storage;
        }

        public ComprobanteFiscal TimbrarComprobante(TimbradoRequest request)
        {
            // Validación de credenciales si se suministran en la petición SOAP
            if (!string.IsNullOrEmpty(request.UsuarioAuth))
            {
                if (request.UsuarioAuth != "admin" || request.PasswordAuth != "admin_pass_2026")
                {
                    throw new FaultException("Credenciales de autenticación SOAP inválidas.", new FaultCode("Client.AuthenticationFailed"));
                }
            }

            if (string.IsNullOrWhiteSpace(request.RfcReceptor))
            {
                throw new FaultException("El RFC del receptor es obligatorio para timbrado.", new FaultCode("Client.InvalidData"));
            }

            if (request.Total <= 0)
            {
                throw new FaultException("El total a timbrar debe ser mayor a cero.", new FaultCode("Client.InvalidAmount"));
            }

            return _storage.Timbrar(request);
        }

        public ComprobanteFiscal ConsultarComprobante(string folioFiscal)
        {
            var comp = _storage.ObtenerComprobante(folioFiscal);
            if (comp == null)
            {
                throw new FaultException($"No se encontró ningún comprobante con UUID '{folioFiscal}'", new FaultCode("Client.NotFound"));
            }
            return comp;
        }
    }
}
