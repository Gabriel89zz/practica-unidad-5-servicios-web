using Microsoft.AspNetCore.Mvc;
using FacturacionApi.Models;
using FacturacionApi.Services;

namespace FacturacionApi.Controllers
{
    [ApiController]
    [Route("api/v1/facturas")]
    [Produces("application/json")]
    public class FacturacionController : ControllerBase
    {
        private readonly FacturaStorage _storage;

        public FacturacionController(FacturaStorage storage)
        {
            _storage = storage;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FacturaModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ListarFacturas([FromQuery] string? rfc, [FromQuery] string? estatus)
        {
            var list = _storage.Listar(rfc, estatus);
            return Ok(list);
        }

        [HttpGet("{folio}")]
        [ProducesResponseType(typeof(FacturaModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerPorFolio(string folio)
        {
            var fac = _storage.ObtenerPorFolio(folio);
            if (fac == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Factura con folio '{folio}' no encontrada.",
                    statusCode = 404
                });
            }
            return Ok(fac);
        }

        [HttpPost]
        [ProducesResponseType(typeof(FacturaModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearFactura([FromBody] CrearFacturaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RfcCliente) || dto.Total <= 0)
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "El RFC del cliente y un total mayor a 0 son obligatorios.",
                    statusCode = 400
                });
            }

            var nueva = _storage.CrearFactura(dto);
            return CreatedAtAction(nameof(ObtenerPorFolio), new { folio = nueva.Folio }, nueva);
        }

        [HttpGet("comprobante/{uuid}")]
        [ProducesResponseType(typeof(ComprobanteFiscal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerComprobante(string uuid)
        {
            var comp = _storage.ObtenerComprobante(uuid);
            if (comp == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Comprobante fiscal con UUID '{uuid}' no encontrado.",
                    statusCode = 404
                });
            }
            return Ok(comp);
        }

        [HttpPut("{folio}")]
        [ProducesResponseType(typeof(FacturaModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ActualizarFactura(string folio, [FromBody] ActualizarFacturaDto dto)
        {
            var f = _storage.ActualizarFactura(folio, dto.Concepto, dto.Estatus);
            if (f == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Factura con folio '{folio}' no encontrada.",
                    statusCode = 404
                });
            }
            return Ok(f);
        }

        [HttpDelete("{folio}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CancelarFactura(string folio)
        {
            if (_storage.CancelarFactura(folio, out var cancelada))
            {
                return Ok(new
                {
                    message = $"Factura con folio '{folio}' cancelada exitosamente ante el SAT.",
                    estatus = "CANCELADA",
                    folioFiscalUUID = cancelada?.FolioFiscalUUID,
                    statusCode = 200
                });
            }

            return NotFound(new
            {
                error = "Not Found",
                message = $"Factura con folio '{folio}' no encontrada.",
                statusCode = 404
            });
        }
    }
}

