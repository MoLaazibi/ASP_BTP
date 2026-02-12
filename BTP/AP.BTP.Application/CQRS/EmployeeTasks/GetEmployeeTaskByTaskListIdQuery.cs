using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class GetEmployeeTaskByTaskListIdQuery : IRequest<IEnumerable<EmployeeTaskDTO>>
    {
        public int TaskListId { get; set; }
    }
    public class GetEmployeeTaskByTaskListIdQueryHandler : IRequestHandler<GetEmployeeTaskByTaskListIdQuery, IEnumerable<EmployeeTaskDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetEmployeeTaskByTaskListIdQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeTaskDTO>> Handle(GetEmployeeTaskByTaskListIdQuery request, CancellationToken cancellationToken)
        {
            var employeeTaskLists = await _unitofWork.EmployeeTaskRepository.GetByTaskListId(request.TaskListId);
            var dtos = _mapper.Map<IEnumerable<EmployeeTaskDTO>>(employeeTaskLists).ToList();

            foreach (var dto in dtos)
            {
                var matchingEntity = employeeTaskLists.FirstOrDefault(t => t.Id == dto.Id);
                if (matchingEntity?.TaskList?.Date.Date > DateTime.Today)
                {
                    dto.StopTime = null;
                }
            }

            return dtos;
        }
    }
}
