using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jemar.Presentation.Authorization;

namespace Jemar.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.SuperAdminOnly)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<UserResponse>> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
        {
            var user = await _userService.CreateAsync(request);
            return Ok(user);
        }

        [HttpPut("role")]
        public async Task<IActionResult> UpdateRole(UpdateUserRoleRequest request)
        {
            await _userService.UpdateRoleAsync(request);
            return NoContent();
        }

        [HttpDelete("{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            await _userService.DeleteAsync(email);
            return NoContent();
        }

    }
}
