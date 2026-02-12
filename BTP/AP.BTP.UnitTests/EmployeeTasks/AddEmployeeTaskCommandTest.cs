using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;


namespace AP.BTP.Tests.Application.EmployeeTasks
{
	[TestClass]
	public class AddEmployeeTaskCommandTest
	{
		private Mock<IUnitofWork> _uowMock;
		private AddEmployeeTaskCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();

			var repo = new Mock<IEmployeeTaskRepository>();
			_uowMock.SetupGet(u => u.EmployeeTaskRepository).Returns(repo.Object);

			_validator = new AddEmployeeTaskCommandValidator(_uowMock.Object);
		}

		private AddEmployeeTaskCommand CreateValidCommand()
		{
			return new AddEmployeeTaskCommand
			{
				EmployeeTask = new EmployeeTaskDTO
				{
					Description = "Task A",
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
			var cmd = CreateValidCommand();
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task MissingDescription_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.Description = "";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.Description");
		}

		[TestMethod]
		public async Task DurationZero_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.PlannedDuration = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.PlannedDuration");
		}

		[TestMethod]
		public async Task MissingStartTime_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.PlannedStartTime = default;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.PlannedStartTime");
		}

		[TestMethod]
		public async Task StartTimeAfterStopTime_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.StartTime = DateTime.Today.AddHours(10);
			cmd.EmployeeTask.StopTime = DateTime.Today.AddHours(9);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.StartTime");
		}

		[TestMethod]
		public async Task InvalidTaskListId_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.TaskListId = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.TaskListId");
		}

		[TestMethod]
		public async Task MissingCategory_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.CategoryId = null;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.CategoryId");
		}

		[TestMethod]
		public async Task InvalidCategoryId_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.EmployeeTask.CategoryId = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("EmployeeTask.CategoryId");
		}

		[TestMethod]
		public async Task Handler_Creates_Task_And_Commits()
		{
			var taskRepo = new Mock<IEmployeeTaskRepository>();
			_uowMock.Setup(u => u.EmployeeTaskRepository).Returns(taskRepo.Object);
			taskRepo.Setup(r => r.Create(It.IsAny<EmployeeTask>()))
				.ReturnsAsync((EmployeeTask e) => e);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<EmployeeTaskDTO>(It.IsAny<EmployeeTask>()))
				.Returns(new EmployeeTaskDTO { Id = 10, Description = "Task A" });

			var handler = new AddEmployeeTaskCommandHandler(_uowMock.Object, mapper.Object);
			var cmd = CreateValidCommand();

			var result = await handler.Handle(cmd, CancellationToken.None);

			taskRepo.Verify(r => r.Create(It.IsAny<EmployeeTask>()), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(10, result.Data.Id);
		}
	}
}
