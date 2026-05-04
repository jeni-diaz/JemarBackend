using Jemar.Aplication.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Jemar.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly ShipmentService _service;

        public ShipmentController(ShipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateShipmentRequest request)
        {
            await _service.CreateAsync(request);
            return Ok();
        }
    }
}
