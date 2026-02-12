using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
	public class UpdateStopTimeCommandTest
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IEmployeeTaskRepository> _taskRepo;
		private Mock<ITaskListRepository> _taskListRepo;
		private Mock<IMapper> _mapper;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_taskRepo = new Mock<IEmployeeTaskRepository>();
			_taskListRepo = new Mock<ITaskListRepository>();
			_mapper = new Mock<IMapper>();

			_uowMock.SetupGet(u => u.EmployeeTaskRepository).Returns(_taskRepo.Object);
			_uowMock.SetupGet(u => u.TaskListRepository).Returns(_taskListRepo.Object);
		}

		[TestMethod]
		public async Task Handler_Sets_StopTime_Updates_And_Commits()
		{
			var task = new EmployeeTask { Id = 5, TaskListId = null };

			_taskRepo.Setup(r => r.GetById(5)).ReturnsAsync(task);
			_mapper.Setup(m => m.Map<EmployeeTaskDTO>(task)).Returns(new EmployeeTaskDTO { Id = 5 });

			var handler = new UpdateStopTimeCommandHandler(_uowMock.Object, _mapper.Object);
			var result = await handler.Handle(new UpdateStopTimeCommand { Id = 5 }, CancellationToken.None);

			_taskRepo.Verify(r => r.Update(task), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);
			Assert.IsNotNull(task.StopTime);
			Assert.AreEqual(5, result.Id);
		}

		[TestMethod]
		public async Task Handler_Archives_TaskList_When_All_Tasks_Completed()
		{
			var task = new EmployeeTask { Id = 5, TaskListId = 10 };
			var otherTask = new EmployeeTask { Id = 6, StopTime = DateTime.Now };
			var list = new TaskList { Id = 10, Date = DateTime.Today, IsArchived = false };

			_taskRepo.Setup(r => r.GetById(5)).ReturnsAsync(task);
			_taskRepo.Setup(r => r.GetByTaskListId(10)).ReturnsAsync(new List<EmployeeTask>
			{
				new EmployeeTask { Id = 5, StopTime = DateTime.Now },
				otherTask
			});

			_taskListRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(list);

			_mapper.Setup(m => m.Map<EmployeeTaskDTO>(task)).Returns(new EmployeeTaskDTO());

			var handler = new UpdateStopTimeCommandHandler(_uowMock.Object, _mapper.Object);
			await handler.Handle(new UpdateStopTimeCommand { Id = 5 }, CancellationToken.None);

			Assert.IsTrue(list.IsArchived);

			_taskListRepo.Verify(r => r.Update(list), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Exactly(2));
		}

		[TestMethod]
		public async Task Handler_DoesNot_Archive_When_Not_All_Tasks_Completed()
		{
			var task = new EmployeeTask { Id = 5, TaskListId = 10 };
			var otherTask = new EmployeeTask { Id = 6, StopTime = null };

			_taskRepo.Setup(r => r.GetById(5)).ReturnsAsync(task);
			_taskRepo.Setup(r => r.GetByTaskListId(10)).ReturnsAsync(new List<EmployeeTask>
			{
				new EmployeeTask { Id = 5, StopTime = DateTime.Now },
				otherTask
			});

			_mapper.Setup(m => m.Map<EmployeeTaskDTO>(task)).Returns(new EmployeeTaskDTO());

			var handler = new UpdateStopTimeCommandHandler(_uowMock.Object, _mapper.Object);
			await handler.Handle(new UpdateStopTimeCommand { Id = 5 }, CancellationToken.None);

			_taskListRepo.Verify(r => r.Update(It.IsAny<TaskList>()), Times.Never);
			_uowMock.Verify(u => u.Commit(), Times.Once);
		}
	}
}
