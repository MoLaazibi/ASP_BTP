using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AP.BTP.Application.CQRS.TaskLists;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.TaskLists
{
	[TestClass]
	public class ArchiveTaskListCommandHandlerTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITaskListRepository> _taskRepo;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<ITaskListRepository>();

			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskRepo.Object);
		}

		private ArchiveTaskListCommand Cmd(int id)
			=> new ArchiveTaskListCommand { TaskListId = id };

		private TaskList Existing(bool hasOpenTasks = false)
		{
			return new TaskList
			{
				Id = 10,
				IsArchived = false,
				Tasks = hasOpenTasks
					? new List<EmployeeTask> { new EmployeeTask { StopTime = null } }
					: new List<EmployeeTask> { new EmployeeTask { StopTime = DateTime.Now } }
			};
		}

		[TestMethod]
		public async Task NotFound_ShouldFail()
		{
			var cmd = Cmd(10);

			_taskRepo.Setup(r => r.GetByIdWithDetails(10))
				.ReturnsAsync((TaskList)null);

			var handler = new ArchiveTaskListCommandHandler(_uow.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task CannotArchive_WhenOpenTasksExist()
		{
			var cmd = Cmd(10);

			var existing = Existing(hasOpenTasks: true);

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			var handler = new ArchiveTaskListCommandHandler(_uow.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Archive_Succeeds_WhenNoOpenTasks()
		{
			var cmd = Cmd(10);

			var existing = Existing(hasOpenTasks: false);

			_taskRepo.Setup(r => r.GetByIdWithDetails(10)).ReturnsAsync(existing);

			var handler = new ArchiveTaskListCommandHandler(_uow.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.IsTrue(existing.IsArchived);
			_taskRepo.Verify(r => r.Update(existing), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
