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
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            var inquiry = await _inquiryService.GetByIdAsync(id, userId, role);
            return Ok(inquiry);
        }

        [HttpPost]
        public async Task<ActionResult<InquiryResponse>> Create(CreateInquiryRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var inquiry = await _inquiryService.CreateAsync(request, userId);
            return Ok(inquiry);
        }

        [HttpPut("{id}/respond")]
        public async Task<IActionResult> Respond(Guid id, RespondInquiryRequest request)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            await _inquiryService.RespondAsync(id, request, userId, role);
            return NoContent();
        }

        [HttpPut("{id}/close")]
        [Authorize(Policy = Policies.EmployeeOrAbove)]
        public async Task<IActionResult> Close(Guid id)
        {
            await _inquiryService.CloseAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = HttpContext.Items["UserId"] as Guid? ?? Guid.Empty;
            var role = HttpContext.Items["UserRole"] as string ?? string.Empty;

            await _inquiryService.DeleteAsync(id, userId, role);
            return NoContent();
        }
    }
}