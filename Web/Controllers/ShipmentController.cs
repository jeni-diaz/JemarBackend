using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Presentation.Authorization;

namespace Jemar.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ShipmentResponse>>> GetAll()
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var shipments = await _shipmentService.GetAllAsync(userId, role);
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentResponse>> GetById(Guid id)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var shipment = await _shipmentService.GetByIdAsync(id, userId, role);
            return Ok(shipment);
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<ShipmentTypeResponse>>> GetShipmentTypes()
        {
            var types = await _shipmentService.GetShipmentTypesAsync();
            return Ok(types);
        }

        [HttpGet("package-sizes")]
        public async Task<ActionResult<List<PackageSizeResponse>>> GetPackageSizes()
        {
            var sizes = await _shipmentService.GetPackageSizesAsync();
            return Ok(sizes);
        }

        [HttpGet("address-search")]
        public async Task<ActionResult<List<GeocodeResult>>> SearchAddresses([FromQuery] string q)
        {
            var results = await _shipmentService.SearchAddressesAsync(q);
            return Ok(results);
        }

        [HttpGet("clients")]
        [Authorize(Policy = Policies.EmployeeOrAbove)]
        public async Task<ActionResult<List<UserResponse>>> GetClients()
        {
            var clients = await _shipmentService.GetClientsAsync();
            return Ok(clients);
        }

        [HttpGet("clients/email-exists")]
        [Authorize(Policy = Policies.EmployeeOrAbove)]
        public async Task<ActionResult<EmailAvailabilityResponse>> CheckEmail([FromQuery] string email)
        {
            var result = await _shipmentService.CheckEmailAsync(email);
            return Ok(result);
        }

        [HttpPost("clients")]
        [Authorize(Policy = Policies.EmployeeOrAbove)]
        public async Task<ActionResult<UserResponse>> CreateClient(SignUpRequest request)
        {
            var client = await _shipmentService.CreateClientAsync(request);
            return Ok(client);
        }

        [HttpPost("quote")]
        public async Task<ActionResult<ShipmentQuoteResponse>> Quote(CreateShipmentRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var quote = await _shipmentService.QuoteAsync(request, userId, role);
            return Ok(quote);
        }

        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> Create(CreateShipmentRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var shipment = await _shipmentService.CreateAsync(request, userId, role);
            return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = Policies.ClientOrAbove)]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateShipmentRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            await _shipmentService.UpdateStatusAsync(id, request, userId, role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            await _shipmentService.DeleteAsync(id, userId, role);
            return NoContent();
        }
    }
}