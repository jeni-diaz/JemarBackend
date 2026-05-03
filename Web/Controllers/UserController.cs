using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jemar.Aplication.Abstractions;
using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;

namespace Jemar.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public ActionResult<List<UserResponse>> GetAll()
        {
            var users = _userService.GetAll();

            if (!users.Any())
                return NotFound();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<UserResponse> GetById(Guid id)
        {
            var user = _userService.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public ActionResult<UserResponse> Create(CreateUserRequest request)
        {
            var user = _userService.Create(request);

            return Ok(user);
        }
    }
}
