using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class EditNurserySiteArchivedCommandValidatorTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<INurserySiteRepository> _siteRepo;
		private EditNurserySiteArchivedCommandValidator _validator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_siteRepo = new Mock<INurserySiteRepository>();

			_uow.SetupGet(x => x.NurserySiteRepository).Returns(_siteRepo.Object);

			_validator = new EditNurserySiteArchivedCommandValidator(_uow.Object);
		}

		[TestMethod]
		public async Task Valid_ShouldPass()
		{
			var cmd = new EditNurserySiteArchivedCommand { Id = 10, IsArchived = true };

			_siteRepo.Setup(r => r.GetById(cmd.Id))
				.ReturnsAsync(new NurserySite { Id = 10 });

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task IdZero_ShouldFail()
		{
			var cmd = new EditNurserySiteArchivedCommand { Id = 0 };

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor(c => c.Id);
		}

		[TestMethod]
		public async Task SiteNotFound_ShouldFail()
		{
			var cmd = new EditNurserySiteArchivedCommand { Id = 10 };

			_siteRepo.Setup(r => r.GetById(10)).ReturnsAsync((NurserySite)null);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor(c => c.Id);
		}
	}

	[TestClass]
	public class EditNurserySiteArchivedCommandHandlerTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<INurserySiteRepository> _siteRepo;
		private Mock<ITaskListRepository> _taskListRepo;
		private Mock<IMapper> _mapper;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_siteRepo = new Mock<INurserySiteRepository>();
			_taskListRepo = new Mock<ITaskListRepository>();
			_mapper = new Mock<IMapper>();

			_uow.SetupGet(x => x.NurserySiteRepository).Returns(_siteRepo.Object);
			_uow.SetupGet(x => x.TaskListRepository).Returns(_taskListRepo.Object);
		}

		private EditNurserySiteArchivedCommand Cmd(bool archived = true)
			=> new EditNurserySiteArchivedCommand { Id = 10, IsArchived = archived };

		private NurserySite Existing(bool withZones = false)
		{
			return new NurserySite
			{
				Id = 10,
				IsArchived = false,
				Zones = withZones
					? new List<Zone> { new Zone { Id = 5 } }
					: new List<Zone>()
			};
		}

		[TestMethod]
		public async Task NotFound_ShouldFail()
		{
			var cmd = Cmd();

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.Id))
				.ReturnsAsync((NurserySite)null);

			var handler = new EditNurserySiteArchivedCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task CannotArchive_When_OpenTasksExist()
		{
			var cmd = Cmd(true);

			var site = Existing(withZones: true);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.Id)).ReturnsAsync(site);

			_taskListRepo.Setup(r => r.GetByZoneIdsWithTasks(new List<int> { 5 }))
				.ReturnsAsync(new List<TaskList>
				{
					new TaskList
					{
						Tasks = new List<EmployeeTask>
						{
							new EmployeeTask { StopTime = null }
						}
					}
				});

			var handler = new EditNurserySiteArchivedCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Archive_Succeeds_When_No_OpenTasks()
		{
			var cmd = Cmd(true);

			var site = Existing(withZones: true);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.Id)).ReturnsAsync(site);

			_taskListRepo.Setup(r => r.GetByZoneIdsWithTasks(new List<int> { 5 }))
				.ReturnsAsync(new List<TaskList>()); // geen taken → OK

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site))
				.Returns(new NurserySiteDTO { Id = site.Id });

			var handler = new EditNurserySiteArchivedCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.IsTrue(site.IsArchived);
			_uow.Verify(u => u.Commit(), Times.Once);
		}

		[TestMethod]
		public async Task Unarchive_Always_Succeeds()
		{
			var cmd = Cmd(false);

			var site = Existing(withZones: true);
			site.IsArchived = true;

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.Id)).ReturnsAsync(site);

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site))
				.Returns(new NurserySiteDTO { Id = site.Id });

			var handler = new EditNurserySiteArchivedCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.IsFalse(site.IsArchived);
			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
