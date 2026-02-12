using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.TaskLists;
using AP.BTP.Application.Interfaces;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.EmployeeTasks
{
	[TestClass]
	public class DeleteEmployeeTaskCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<IEmployeeTaskRepository> _taskRepo;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_taskRepo = new Mock<IEmployeeTaskRepository>();

			_uow.SetupGet(u => u.EmployeeTaskRepository).Returns(_taskRepo.Object);
		}

		[TestMethod]
		public async Task Handle_TaskNotFound_ReturnsFailure()
		{
			_taskRepo.Setup(r => r.GetById(5)).ReturnsAsync((EmployeeTask)null);

			var handler = new DeleteEmployeeTaskCommandHandler(_uow.Object);
			var result = await handler.Handle(
				new DeleteEmployeeTaskCommand { EmployeeTask = new EmployeeTaskDTO { Id = 5 } },
				CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
			_taskRepo.Verify(r => r.Delete(It.IsAny<EmployeeTask>()), Times.Never);
			_uow.Verify(u => u.Commit(), Times.Never);
		}

		[TestMethod]
		public async Task Handle_Deletes_And_Commits()
		{
			var existing = new EmployeeTask { Id = 5 };

			_taskRepo.Setup(r => r.GetById(5)).ReturnsAsync(existing);

			var handler = new DeleteEmployeeTaskCommandHandler(_uow.Object);
			var result = await handler.Handle(
				new DeleteEmployeeTaskCommand { EmployeeTask = new EmployeeTaskDTO { Id = 5 } },
				CancellationToken.None);

			_taskRepo.Verify(r => r.Delete(existing), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(5, result.Data.Id);
		}
	}
}
