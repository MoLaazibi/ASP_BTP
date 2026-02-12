using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.TaskLists;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;

namespace AP.BTP.Tests.Application.TaskLists
{
	[TestClass]
	public class AddTaskListCommandValidatorTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITaskListRepository> _taskRepo;
		private Mock<IZoneRepository> _zoneRepo;

		private AddTaskListCommandValidator _validator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<ITaskListRepository>();
			_zoneRepo = new Mock<IZoneRepository>();

			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskRepo.Object);
			_uow.SetupGet(x => x.ZoneRepository).Returns(_zoneRepo.Object);

			_validator = new AddTaskListCommandValidator(_uow.Object);
		}

		private AddTaskListCommand Valid()
		{
			return new AddTaskListCommand
			{
				TaskList = new TaskListDTO
				{
					Id = 0,
					UserId = 10,
					ZoneId = 5,
					Date = new DateTime(2025, 3, 20),
				},
				Tasks = new List<EmployeeTaskDTO>
				{
					new EmployeeTaskDTO
					{
						Description = "Task A",
						PlannedDuration = 1,
						PlannedStartTime = new DateTime(2025,3,20,8,0,0),
						Order = 0,
						CategoryId = 2
					}
				}
			};
		}

		[TestMethod]
		public async Task Valid_ShouldPass()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetCountByUserAndWeek(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(0);

			_zoneRepo.Setup(r => r.GetByIdWithNurserySite(cmd.TaskList.ZoneId.Value))
				.ReturnsAsync(new Zone { Id = 5, NurserySite = new NurserySite { Id = 1 } });

			_taskRepo.Setup(r => r.GetByUserAndWeekWithZoneAndNurserySite(10, 2025, 12))
				.ReturnsAsync(new List<TaskList>());

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task MissingTasks_ShouldFail()
		{
			var cmd = Valid();
			cmd.Tasks = new List<EmployeeTaskDTO>();

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Tasks");
		}

		[TestMethod]
		public async Task MoreThanFourTasks_ShouldFail()
		{
			var cmd = Valid();

			cmd.Tasks = Enumerable.Range(1, 5)
				.Select(i => new EmployeeTaskDTO
				{
					Description = "T",
					PlannedDuration = 1,
					PlannedStartTime = DateTime.Now.AddHours(i),
					Order = i,
					CategoryId = 1
				}).ToList();

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Tasks.Count");
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
					CategoryId = 2
				},
				new EmployeeTaskDTO
				{
					Description = "B",
					PlannedDuration = 1,
					PlannedStartTime = new DateTime(2025,3,20,8,30,0),
					Order = 1,
					CategoryId = 2
				}
			};

			var result = await _validator.TestValidateAsync(cmd);
			Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("overlappende")));
		}

		[TestMethod]
		public async Task UserAlreadyHasFiveInWeek_ShouldFail()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.GetCountByUserAndWeek(10, 2025, It.IsAny<int>()))
				.ReturnsAsync(5);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.UserId");
		}

		[TestMethod]
		public async Task UserHasTaskListInOtherNurserySite_ShouldFail()
		{
			var cmd = Valid();

			_zoneRepo.Setup(r => r.GetByIdWithNurserySite(cmd.TaskList.ZoneId.Value))
				.ReturnsAsync(new Zone { Id = 5, NurserySite = new NurserySite { Id = 1 } });

			_taskRepo.Setup(r => r.GetByUserAndWeekWithZoneAndNurserySite(10, 2025, It.IsAny<int>()))
				.ReturnsAsync(new List<TaskList>
				{
					new TaskList
					{
						Zone = new Zone { Id = 9, NurserySite = new NurserySite { Id = 999 } }
					}
				});

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("TaskList.UserId");
		}
	}

	[TestClass]
	public class AddTaskListCommandHandlerTests
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITaskListRepository> _taskRepo;
		private Mock<IMapper> _mapper;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<ITaskListRepository>();
			_mapper = new Mock<IMapper>();

			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskRepo.Object);
		}

		private AddTaskListCommand Valid()
		{
			return new AddTaskListCommand
			{
				TaskList = new TaskListDTO
				{
					UserId = 10,
					ZoneId = 5,
					Date = new DateTime(2025, 3, 20)
				},
				Tasks = new List<EmployeeTaskDTO>
				{
					new EmployeeTaskDTO
					{
						Description = "A",
						PlannedDuration = 1,
						PlannedStartTime = new DateTime(2025,3,20,8,0,0),
						Order = 0,
						CategoryId = 1
					}
				}
			};
		}

		[TestMethod]
		public async Task Creates_And_Commits()
		{
			var cmd = Valid();

			_taskRepo.Setup(r => r.Create(It.IsAny<TaskList>()))
				.ReturnsAsync((TaskList t) => t);

			var handler = new AddTaskListCommandHandler(_uow.Object, _mapper.Object);

			_mapper.Setup(m => m.Map<TaskListDTO>(It.IsAny<TaskList>()))
				.Returns(new TaskListDTO { Id = 123 });

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_taskRepo.Verify(r => r.Create(It.IsAny<TaskList>()), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
