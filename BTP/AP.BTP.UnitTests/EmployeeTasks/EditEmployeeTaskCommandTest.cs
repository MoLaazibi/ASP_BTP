using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;


namespace AP.BTP.Tests.Application.EmployeeTasks
{
	[TestClass]
	public class EditEmployeeTaskCommandTest
	{
		private Mock<IUnitofWork> _uowMock;
		private EditEmployeeTaskCommandValidator _validator;
		private Mock<IEmployeeTaskRepository> _taskRepo;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_taskRepo = new Mock<IEmployeeTaskRepository>();

			_uowMock.SetupGet(u => u.EmployeeTaskRepository).Returns(_taskRepo.Object);
			_validator = new EditEmployeeTaskCommandValidator(_uowMock.Object);
		}

		private EditEmployeeTaskCommand CreateValidCommand()
		{
			return new EditEmployeeTaskCommand
			{
				EmployeeTask = new EmployeeTaskDTO
				{
					Id = 10,
					Description = "Updated description",
					PlannedDuration = 60,
					PlannedStartTime = DateTime.Today.AddHours(8),
					StartTime = null,
					StopTime = null,
					Order = 1,
					TaskListId = 5,
					CategoryId = 3
				}
			};
		}

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task TaskDoesNotExist_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync((EmployeeTask)null);

			var cmd = CreateValidCommand();
			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("EmployeeTask.Id");
		}

		[TestMethod]
		public async Task MissingDescription_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.Description = "";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.Description");
		}

		[TestMethod]
		public async Task InvalidDuration_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.PlannedDuration = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.PlannedDuration");
		}

		[TestMethod]
		public async Task MissingStartTime_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.PlannedStartTime = default;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.PlannedStartTime");
		}

		[TestMethod]
		public async Task StartAfterStop_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.StartTime = DateTime.Today.AddHours(10);
			cmd.EmployeeTask.StopTime = DateTime.Today.AddHours(9);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.StartTime");
		}

		[TestMethod]
		public async Task InvalidTaskListId_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.TaskListId = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.TaskListId");
		}

		[TestMethod]
		public async Task MissingCategory_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.CategoryId = null;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.CategoryId");
		}

		[TestMethod]
		public async Task InvalidCategoryId_ShouldFail()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(new EmployeeTask { Id = 10 });

			var cmd = CreateValidCommand();
			cmd.EmployeeTask.CategoryId = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.CategoryId");
		}

		[TestMethod]
		public async Task Handler_Updates_Task_And_Commits()
		{
			var existing = new EmployeeTask
			{
				Id = 10,
				Description = "Old",
				PlannedDuration = 30
			};

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(existing);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<EmployeeTaskDTO>(existing))
				.Returns(new EmployeeTaskDTO { Id = 10, Description = "Updated description" });

			var handler = new EditEmployeeTaskCommandHandler(_uowMock.Object, mapper.Object);

			var result = await handler.Handle(CreateValidCommand(), CancellationToken.None);

			_taskRepo.Verify(r => r.Update(existing), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(10, result.Data.Id);
		}

		[TestMethod]
		public async Task Handler_TaskNotFound_ReturnsFailure()
		{
			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync((EmployeeTask)null);

			var mapper = new Mock<IMapper>();
			var handler = new EditEmployeeTaskCommandHandler(_uowMock.Object, mapper.Object);

			var result = await handler.Handle(CreateValidCommand(), CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}
	}
}
