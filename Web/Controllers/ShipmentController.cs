using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var shipment = await _shipmentService.GetByIdAsync(id, userId, role);
                if (shipment == null)
                    return NotFound();

                return Ok(shipment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> Create(CreateShipmentRequest request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var shipment = await _shipmentService.CreateAsync(request, userId);
                return Ok(shipment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "EmployeeOrAbove")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateShipmentRequest request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var result = await _shipmentService.UpdateStatusAsync(id, request, userId, role);
                if (!result)
                    return NotFound("Shipment not found.");

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var result = await _shipmentService.DeleteAsync(id, userId, role);
                if (!result)
                    return NotFound("Shipment not found.");

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}