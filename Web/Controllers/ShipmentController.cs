using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;

namespace Jemar.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _ShipmentoService;

        public ShipmentController(IShipmentService ShipmentoService)
        {
            _ShipmentoService = ShipmentoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ShipmentResponse>>> GetAll()
        {
            var shipments = await _ShipmentoService.GetAll();
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentResponse>> GetById(Guid id)
        {
            var shipment = await _ShipmentoService.GetById(id);

            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> Create(CreateShipmentRequest request)
        {
            var shipment = await _ShipmentoService.Create(request);
            return Ok(shipment);
        }
    }
}