using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.TaskLists;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using MediatR;
using AP.BTP.Application.CQRS;

namespace AP.BTP.Tests.Application.TaskLists
{
	[TestClass]
	public class EditTaskListCommandValidatorTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITaskListRepository> _taskRepo;
		private Mock<IZoneRepository> _zoneRepo;

		private EditTaskListCommandValidator _validator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<ITaskListRepository>();
			_zoneRepo = new Mock<IZoneRepository>();

			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskRepo.Object);
			_uow.SetupGet(x => x.ZoneRepository).Returns(_zoneRepo.Object);

			_validator = new EditTaskListCommandValidator(_uow.Object);
		}

		private EditTaskListCommand Valid()
		{
			return new EditTaskListCommand
			{
				TaskList = new TaskListDTO
				{
					Id = 10,
					UserId = 20,
					ZoneId = 5,
					Date = new DateTime(2025, 3, 20)
				},
				Tasks = new List<EmployeeTaskDTO>
				{
					new EmployeeTaskDTO
					{
						Id = 1,
						Description = "Task A",
						PlannedDuration = 1,
						PlannedStartTime = new DateTime(2025,3,20,8,0,0),
						Order = 0,
						CategoryId = 1
					}
				}
			};
		}

		[TestMethod]
		public async Task Valid_ShouldPass()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new TaskList { Id = 10 });
			_taskRepo.Setup(r => r.GetCountByUserAndWeek(20, 2025, It.IsAny<int>())).ReturnsAsync(0);

			_zoneRepo.Setup(r => r.GetByIdWithNurserySite(5))
				.ReturnsAsync(new Zone { Id = 5, NurserySite = new NurserySite { Id = 1 } });

			_taskRepo.Setup(r => r.GetByUserAndWeekWithZoneAndNurserySite(20, 2025, It.IsAny<int>()))
				.ReturnsAsync(new List<TaskList>());

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			var cmd = Valid();
			cmd.TaskList.Id = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.Id");
		}

		[TestMethod]
		public async Task TaskListNotFound_ShouldFail()
		{
			var cmd = Valid();
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync((TaskList)null);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.Id");
		}

		[TestMethod]
		public async Task OverlappingTasks_ShouldFail()
		{
			var cmd = Valid();

			cmd.Tasks = new List<EmployeeTaskDTO>
			{
				new EmployeeTaskDTO
				{
					Description = "A",
					PlannedDuration = 2,
					PlannedStartTime = new DateTime(2025,3,20,8,0,0),
					Order = 0,
					CategoryId = 1
				},
				new EmployeeTaskDTO
				{
					Description = "B",
					PlannedDuration = 1,
					PlannedStartTime = new DateTime(2025,3,20,8,30,0),
					Order = 1,
					CategoryId = 1
				}
			};

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new TaskList { Id = 10 });

			var result = await _validator.TestValidateAsync(cmd);
			Assert.IsTrue(result.Errors.Any());
		}

		[TestMethod]
		public async Task UserHasFiveTaskListsInWeek_ShouldFail()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new TaskList { Id = 10 });
			_taskRepo.Setup(r => r.GetCountByUserAndWeek(20, 2025, It.IsAny<int>()))
				.ReturnsAsync(5);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.UserId");
		}

		[TestMethod]
		public async Task DifferentNurserySiteInSameWeek_ShouldFail()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new TaskList { Id = 10 });

			_zoneRepo.Setup(r => r.GetByIdWithNurserySite(5))
				.ReturnsAsync(new Zone { Id = 5, NurserySite = new NurserySite { Id = 1 } });

			_taskRepo.Setup(r => r.GetByUserAndWeekWithZoneAndNurserySite(20, 2025, It.IsAny<int>()))
				.ReturnsAsync(new List<TaskList>
				{
					new TaskList {
						Id = 99,
						Zone = new Zone { NurserySite = new NurserySite { Id = 999 } }
					}
				});

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.UserId");
		}
	}

	[TestClass]
	public class EditTaskListCommandHandlerTests
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITaskListRepository> _taskRepo;
		private Mock<IMapper> _mapper;
		private Mock<IMediator> _mediator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<ITaskListRepository>();
			_mapper = new Mock<IMapper>();
			_mediator = new Mock<IMediator>();

			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskRepo.Object);
		}

		private EditTaskListCommand Valid()
		{
			return new EditTaskListCommand
			{
				TaskList = new TaskListDTO
				{
					Id = 10,
					UserId = 20,
					ZoneId = 5,
					Date = new DateTime(2025, 3, 20)
				},
				Tasks = new List<EmployeeTaskDTO>
				{
					new EmployeeTaskDTO
					{
						Id = 1,
						Description = "A",
						PlannedDuration = 1,
						PlannedStartTime = new DateTime(2025,3,20,8,0,0),
						Order = 0,
						CategoryId = 1
					}
				}
			};
		}

		private TaskList Existing()
		{
			return new TaskList
			{
				Id = 10,
				UserId = 10,
				ZoneId = 3,
				Date = new DateTime(2025, 3, 10),
				Tasks = new List<EmployeeTask>()
			};
		}

		[TestMethod]
		public async Task NotFound_ShouldFail()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10))
				.ReturnsAsync((TaskList)null);

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Updates_Fields()
		{
			var cmd = Valid();
			var existing = Existing();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			_mapper.Setup(m => m.Map<TaskListDTO>(existing)).Returns(cmd.TaskList);

			_mediator.Setup(m => m.Send(It.IsAny<IRequest<Result<EmployeeTaskDTO>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<EmployeeTaskDTO>.Success(new EmployeeTaskDTO(), ""));

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(cmd.TaskList.Date, existing.Date);
			Assert.AreEqual(cmd.TaskList.UserId, existing.UserId);
			Assert.AreEqual(cmd.TaskList.ZoneId, existing.ZoneId);
		}

		[TestMethod]
		public async Task Adds_New_Tasks()
		{
			var cmd = Valid();
			cmd.Tasks = new List<EmployeeTaskDTO>
			{
				new EmployeeTaskDTO
				{
					Id = 0,
					Description = "NEW",
					PlannedDuration = 1,
					PlannedStartTime = DateTime.Now,
					CategoryId = 1
				}
			};

			var existing = Existing();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			_mapper.Setup(m => m.Map<TaskListDTO>(existing)).Returns(cmd.TaskList);

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_mediator.Setup(m => m.Send(It.IsAny<AddEmployeeTaskCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<EmployeeTaskDTO>.Success(new EmployeeTaskDTO(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<AddEmployeeTaskCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Edits_Existing_Tasks()
		{
			var cmd = Valid();
			cmd.Tasks[0].Id = 5;

			var existing = Existing();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			_mapper.Setup(m => m.Map<TaskListDTO>(existing)).Returns(cmd.TaskList);

			_mediator.Setup(m => m.Send(It.IsAny<EditEmployeeTaskCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<EmployeeTaskDTO>.Success(new EmployeeTaskDTO(), ""));

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<EditEmployeeTaskCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Deletes_Tasks()
		{
			var cmd = Valid();
			cmd.Tasks[0].IsDeleted = true;

			var existing = Existing();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			_mapper.Setup(m => m.Map<TaskListDTO>(existing)).Returns(cmd.TaskList);

			_mediator.Setup(m => m.Send(It.IsAny<DeleteEmployeeTaskCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<EmployeeTaskDTO>.Success(new EmployeeTaskDTO(), ""));

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<DeleteEmployeeTaskCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Commits_When_Done()
		{
			var cmd = Valid();
			var existing = Existing();

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			_mapper.Setup(m => m.Map<TaskListDTO>(existing)).Returns(cmd.TaskList);

			_mediator.Setup(m => m.Send(It.IsAny<IRequest<Result<EmployeeTaskDTO>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<EmployeeTaskDTO>.Success(new EmployeeTaskDTO(), ""));

			var handler = new EditTaskListCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
