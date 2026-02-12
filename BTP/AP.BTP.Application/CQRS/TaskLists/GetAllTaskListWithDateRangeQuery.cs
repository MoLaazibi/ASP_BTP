using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.BTP.Application.CQRS.TaskLists
{
	public class GetAllTaskListWithDateRangeQuery : IRequest<IEnumerable<TaskListDTO>>
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
	public class GetAllTaskListWithDateRangeQueryHandler : IRequestHandler<GetAllTaskListWithDateRangeQuery, IEnumerable<TaskListDTO>> 
	{
		private readonly IUnitofWork _unitofWork;
		private readonly IMapper _mapper;
		public GetAllTaskListWithDateRangeQueryHandler(IUnitofWork unitofWork, IMapper mapper)
		{
			_unitofWork = unitofWork;
			_mapper = mapper;
		}
		public async Task<IEnumerable<TaskListDTO>> Handle(GetAllTaskListWithDateRangeQuery request, CancellationToken cancellationToken)
		{
			var taskLists = await _unitofWork.TaskListRepository.GetAllWithDateRange(request.Start, request.End);
			return _mapper.Map<IEnumerable<TaskListDTO>>(taskLists);
		}
	}
}
