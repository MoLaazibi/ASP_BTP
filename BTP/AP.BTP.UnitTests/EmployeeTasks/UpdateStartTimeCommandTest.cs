using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;

namespace AP.BTP.Tests.Application.EmployeeTasks
{
	[TestClass]
	public class UpdateStartTimeCommandTest
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IEmployeeTaskRepository> _taskRepo;
		private Mock<ITaskListRepository> _taskListRepo;
		private UpdateStartTimeCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_taskRepo = new Mock<IEmployeeTaskRepository>();
			_taskListRepo = new Mock<ITaskListRepository>();

			_uowMock.SetupGet(u => u.EmployeeTaskRepository).Returns(_taskRepo.Object);
			_uowMock.SetupGet(u => u.TaskListRepository).Returns(_taskListRepo.Object);

			_validator = new UpdateStartTimeCommandValidator(_uowMock.Object);
		}

		private EmployeeTask CreateTask(int id, int order, int? taskListId = 1)
		{
			return new EmployeeTask
			{
				Id = id,
				Order = order,
				TaskListId = taskListId
			};
		}

		private TaskList CreateTaskList(params EmployeeTask[] tasks)
		{
			return new TaskList
			{
				Id = 1,
				Tasks = tasks.ToList()
			};
		}

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			var t1 = CreateTask(5, 1);
			var t2 = CreateTask(10, 2);
			t1.StopTime = DateTime.Now;

			var list = CreateTaskList(t1, t2);

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(t2);
			_taskListRepo.Setup(r => r.GetById(1)).ReturnsAsync(list);

			var cmd = new UpdateStartTimeCommand { Id = 10 };
			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			var cmd = new UpdateStartTimeCommand { Id = 0 };
			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Id");
		}

		[TestMethod]
		public async Task AnotherTaskActive_ShouldFail()
		{
			var t1 = CreateTask(5, 1);
			var t2 = CreateTask(10, 2);

			t1.StartTime = DateTime.Now.AddMinutes(-30);
			t1.StopTime = null;

			var list = CreateTaskList(t1, t2);

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(t2);
			_taskListRepo.Setup(r => r.GetById(1)).ReturnsAsync(list);

			var cmd = new UpdateStartTimeCommand { Id = 10 };
			var result = await _validator.TestValidateAsync(cmd);

			Assert.IsTrue(result.Errors.Any(e =>
				e.ErrorMessage.Contains("taak bezig")));
		}

		[TestMethod]
		public async Task PreviousTasksNotFinished_ShouldFail()
		{
			var t1 = CreateTask(5, 1);
			var t2 = CreateTask(10, 2);

			t1.StartTime = DateTime.Now;
			t1.StopTime = null;

			var list = CreateTaskList(t1, t2);

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(t2);
			_taskListRepo.Setup(r => r.GetById(1)).ReturnsAsync(list);

			var cmd = new UpdateStartTimeCommand { Id = 10 };
			var result = await _validator.TestValidateAsync(cmd);

			Assert.IsTrue(result.Errors.Any(e =>
				e.ErrorMessage.Contains("taak bezig")));
		}

		[TestMethod]
		public async Task Handler_Updates_StartTime_And_Commits()
		{
			var task = CreateTask(10, 2);

			_taskRepo.Setup(r => r.GetById(10)).ReturnsAsync(task);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<EmployeeTaskDTO>(task))
				  .Returns(new EmployeeTaskDTO { Id = 10 });

			var handler = new UpdateStartTimeCommandHandler(_uowMock.Object, mapper.Object);

			var result = await handler.Handle(new UpdateStartTimeCommand { Id = 10 }, CancellationToken.None);

			_taskRepo.Verify(r => r.Update(task), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);

			Assert.IsNotNull(task.StartTime);
			Assert.AreEqual(10, result.Id);
		}
	}
}
