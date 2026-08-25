using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Responses;

namespace Jemar.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("shipment/{shipmentId}/preference")]
        public async Task<ActionResult<CreatePaymentPreferenceResponse>> CreatePreference(Guid shipmentId)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var frontendBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Frontend:BaseUrl"] ?? string.Empty;
            var backendBaseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = await _paymentService.CreatePreferenceAsync(shipmentId, userId, role, frontendBaseUrl, backendBaseUrl);
            return Ok(result);
        }

        [HttpGet("shipment/{shipmentId}/status")]
        public async Task<ActionResult<PaymentStatusResponse>> GetStatus(Guid shipmentId)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var result = await _paymentService.GetStatusAsync(shipmentId, userId, role);
            return Ok(result);
        }

        [HttpPost("sync")]
        public async Task<ActionResult<PaymentStatusResponse>> Sync([FromQuery] long mercadoPagoPaymentId)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var result = await _paymentService.SyncFromMercadoPagoAsync(mercadoPagoPaymentId, userId, role);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromQuery(Name = "data.id")] string? dataId, [FromQuery] string? id, [FromQuery] string? type, [FromQuery] string? topic)
        {
            var isPaymentEvent = type == "payment" || topic == "payment";
            var rawId = dataId ?? id;

            if (isPaymentEvent && long.TryParse(rawId, out var mercadoPagoPaymentId))
            {
                try
                {
                    await _paymentService.SyncFromMercadoPagoAsync(mercadoPagoPaymentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar el webhook de Mercado Pago para el pago {PaymentId}", mercadoPagoPaymentId);
                }
            }

            return Ok();
        }
    }
}
