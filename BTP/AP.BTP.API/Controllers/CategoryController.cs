using AP.BTP.Application.CQRS.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AP.BTP.API.Controllers
{
    public class CategoryController : APIv1Controller
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isArchived = null)
        {
            var query = new GetAllCategoryQuery { PageNr = pageNr, PageSize = pageSize, IsArchived = isArchived };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetCategoryByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] AddCategoryCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            var result = await _mediator.Send(new ArchiveCategoryCommand { CategoryId = id });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EditCategoryCommand command)
        {
            if (id != command.Id)
                return BadRequest("URL ID en body ID komen niet overeen.");

            var result = await _mediator.Send(command);
            if (result == null)
                BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var query = new DeleteCategoryCommand { Id = id };
            var result = await _mediator.Send(query);
            if (result == null) return BadRequest(result);
            return Ok(result);
        }
    }
}
