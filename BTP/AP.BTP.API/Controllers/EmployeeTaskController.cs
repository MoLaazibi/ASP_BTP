using AP.BTP.Application.CQRS.EmployeeTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AP.BTP.API.Controllers
{
    [Authorize(Policy = "AllowAnonymousAccess")]
    [ApiController]
    public class EmployeeTaskController : APIv1Controller
    {
        private readonly IMediator _mediator;
        public EmployeeTaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("taskList/{taskId}")]
        public async Task<IActionResult> GetByTaskListId(int taskId)
        {
            var query = new GetEmployeeTaskByTaskListIdQuery { TaskListId = taskId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("start")]
        public async Task<IActionResult> StartTask([FromBody] UpdateStartTimeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result == null) BadRequest(result);
            return Ok(result);
        }
        [HttpPut("finish")]
        public async Task<IActionResult> FinishTask([FromBody] UpdateStopTimeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result == null) BadRequest(result);
            return Ok(result);
        }
    }

}
