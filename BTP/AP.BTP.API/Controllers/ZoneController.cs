using AP.BTP.Application.CQRS.NurserySites;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AP.BTP.API.Controllers
{
    public class ZoneController : APIv1Controller
    {
        private readonly IMediator mediator;
        public ZoneController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllZoneQuery { PageNr = pageNr, PageSize = pageSize };
            var result = await mediator.Send(query);
            return Ok(result);
        }
    }
}
