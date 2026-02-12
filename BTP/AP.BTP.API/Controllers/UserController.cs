using AP.BTP.API.Services;
using AP.BTP.Application.CQRS.Users;
using AP.BTP.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace AP.BTP.API.Controllers
{
    [ApiController]
    public class UserController : APIv1Controller
    {

        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _mediator.Send(new GetUserCountQuery());
            return Ok(count);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllUserQuery { PageNr = pageNr, PageSize = pageSize };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("byemail")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var result = await _mediator.Send(new GetByEmailQuery { Email = email });
            if (result == null)
                return NotFound(new { message = "User not found." });

            return Ok(result);
        }

        [HttpPut("byemail")]
        public async Task<IActionResult> UpdateByEmail([FromQuery] string email, [FromBody] UpdateUserCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });
            if (cmd == null || cmd.User == null)
                return BadRequest(new { message = "User payload is required." });

            if (string.IsNullOrWhiteSpace(cmd.User.Email))
            {
                cmd.User.Email = email;
            }
            else if (!string.Equals(cmd.User.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Email in query and body do not match." });
            }

            var updated = await _mediator.Send(cmd);
            return Ok(updated);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var rawAuthId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(rawAuthId))
                return Unauthorized();


            var cleanAuthId = rawAuthId.Replace("auth0|", "");

            var query = new GetUserByAuthIdQuery { AuthId = cleanAuthId };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("byrole")]
        public async Task<IActionResult> GetByRole([FromQuery] string role, [FromQuery] int pageNr = 1, [FromQuery] int pageSize = 0)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest(new { message = "Role is required." });

            if (!Enum.TryParse<Role>(role, true, out var parsedRole))
                return BadRequest(new { message = "Role is invalid." });

            var result = await _mediator.Send(new GetUserByRoleQuery { Role = parsedRole, PageNr = pageNr, PageSize = pageSize });
            if (result == null)
                return NotFound(new { message = "No users with this role found" });

            return Ok(result);
        }

        [HttpGet]
        [Route("withoutnurserysitebyrole")]
        public async Task<IActionResult> GetWithoutNurserySiteByRole([FromQuery] string role, [FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest(new { message = "Role is required." });

            if (!Enum.TryParse<Role>(role, true, out var parsedRole))
                return BadRequest(new { message = "Role is invalid." });

            var query = new GetUserWithoutNurserySiteByRoleQuery { PageNr = pageNr, PageSize = pageSize };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "No users with this role found" });

            return Ok(result);
        }
    }
}
