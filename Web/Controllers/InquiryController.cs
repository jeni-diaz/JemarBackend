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
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiryService;

        public InquiryController(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<InquiryResponse>>> GetAll()
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var inquiries = await _inquiryService.GetAllAsync(userId, role);
            return Ok(inquiries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InquiryResponse>> GetById(Guid id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var inquiry = await _inquiryService.GetByIdAsync(id, userId, role);
                if (inquiry == null)
                    return NotFound();

                return Ok(inquiry);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<InquiryResponse>> Create(CreateInquiryRequest request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var inquiry = await _inquiryService.CreateAsync(request, userId);
                return Ok(inquiry);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/respond")]
        public async Task<IActionResult> Respond(Guid id, RespondInquiryRequest request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var result = await _inquiryService.RespondAsync(id, request, userId, role);
                if (!result)
                    return NotFound("Inquiry not found.");

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

        [HttpPut("{id}/close")]
        [Authorize(Policy = "EmployeeOrAbove")]
        public async Task<IActionResult> Close(Guid id)
        {
            var result = await _inquiryService.CloseAsync(id);
            if (!result)
                return NotFound("Inquiry not found.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
                var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

                var result = await _inquiryService.DeleteAsync(id, userId, role);
                if (!result)
                    return NotFound("Inquiry not found.");

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
