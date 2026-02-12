using AP.BTP.Application.CQRS.TaskLists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace AP.BTP.API.Controllers
{

    [Authorize(Policy = "AllowAnonymousAccess")]
    [ApiController]
    public class TaskListController : APIv1Controller
    {
        private readonly IMediator _mediator;
        public TaskListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount([FromQuery] bool? isArchived)
        {
            var count = await _mediator.Send(new GetTaskListCountQuery { IsArchived = isArchived });
            return Ok(count);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isArchived = null)
        {
            var query = new GetAllTaskListQuery { PageNr = pageNr, PageSize = pageSize, IsArchived = isArchived };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("date")]
        public async Task<IActionResult> GetAllWithDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var query = new GetAllTaskListWithDateRangeQuery { Start = start, End = end };
            var result = await _mediator.Send(query);
            if (result == null) return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("userdate")]
        public async Task<IActionResult> GetByUserIdAndDate([FromQuery] int userId, [FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            var query = new GetTaskListByUserIdAndDateQuery { UserId = userId, Date = targetDate };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("userdaterange")]
        public async Task<IActionResult> GetByUserIdAndDateRange([FromQuery] int userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var query = new GetTaskListByUserIdAndDateRangeQuery { UserId = userId, StartDate = start, EndDate = end };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("countbyuserweek")]
        public async Task<IActionResult> GetCountByUserWeek(int userId, DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            int week = culture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday
            );
            int year = date.Year;

            var count = await _mediator.Send(new GetTaskListCountByUserWeekQuery
            {
                UserId = userId,
                Year = year,
                Week = week
            });

            return Ok(count);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUserId(int id)
        {
            var query = new GetTaskListByUserIdQuery { UserId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetTaskListByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] AddTaskListCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Edit([FromBody] EditTaskListCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            var result = await _mediator.Send(new ArchiveTaskListCommand { TaskListId = id });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
    }
}
