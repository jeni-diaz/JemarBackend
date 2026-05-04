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
        private readonly IShipmentService _service; // ✔ interfaz

        public ShipmentController(IShipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ShipmentResponse>>> GetAll()
        {
            var shipments = await _service.GetAll();
            return Ok(shipments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentResponse>> GetById(Guid id)
        {
            var shipment = await _service.GetById(id);

            if (shipment == null)
                return NotFound();

            return Ok(shipment);
        }

        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> Create(CreateShipmentRequest request)
        {
            var shipment = await _service.Create(request);
            return Ok(shipment);
        }
    }
}