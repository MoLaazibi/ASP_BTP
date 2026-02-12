using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.TreeTypes;
using AP.BTP.Application.Interfaces;
using FluentValidation.TestHelper;
using AutoMapper;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.TreeTypes
{
	[TestClass]
	public class EditTreeTypeIsArchivedCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITreeTypeRepository> _treeRepo;
		private Mock<IZoneRepository> _zoneRepo;
		private EditTreeTypeIsArchivedCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_treeRepo = new Mock<ITreeTypeRepository>();
			_zoneRepo = new Mock<IZoneRepository>();

			_uow.SetupGet(u => u.TreeTypeRepository).Returns(_treeRepo.Object);
			_uow.SetupGet(u => u.ZoneRepository).Returns(_zoneRepo.Object);

			_validator = new EditTreeTypeIsArchivedCommandValidator(_uow.Object);
		}

		private EditTreeTypeArchivedCommand Valid()
		{
			return new EditTreeTypeArchivedCommand
			{
				Id = 1,
				IsArchived = true
			};
		}

		// VALIDATOR TESTS

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			_treeRepo.Setup(r => r.GetById(1)).ReturnsAsync(new TreeType { Id = 1 });

			var result = await _validator.TestValidateAsync(Valid());

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetById(0)).ReturnsAsync((TreeType)null);

			var cmd = new EditTreeTypeArchivedCommand { Id = 0, IsArchived = true };

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Id");
		}

		[TestMethod]
		public async Task NonExistingTreeType_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetById(1)).ReturnsAsync((TreeType)null);

			var result = await _validator.TestValidateAsync(Valid());

			result.ShouldHaveValidationErrorFor("Id");
		}

		// HANDLER TESTS

		[TestMethod]
		public async Task Handler_NotFound_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetById(1)).ReturnsAsync((TreeType)null);

			var mapper = new Mock<IMapper>();
			var handler = new EditTreeTypeIsArchivedCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_ArchiveBlocked_DueToLinkedZone_ShouldFail()
		{
			var entity = new TreeType { Id = 1, IsArchived = false };

			_treeRepo.Setup(r => r.GetById(1)).ReturnsAsync(entity);
			_zoneRepo.Setup(z => z.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Zone, bool>>>()))
					 .ReturnsAsync(new Zone { Id = 44 });

			var mapper = new Mock<IMapper>();
			var handler = new EditTreeTypeIsArchivedCommandHandler(_uow.Object, mapper.Object);

			var cmd = Valid(); // IsArchived = true

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
			_treeRepo.Verify(r => r.Update(It.IsAny<TreeType>()), Times.Never);
			_uow.Verify(u => u.Commit(), Times.Never);
		}

		[TestMethod]
		public async Task Handler_Updates_And_Commits()
		{
			var entity = new TreeType { Id = 1, IsArchived = false };

			_treeRepo.Setup(r => r.GetById(1)).ReturnsAsync(entity);
			_zoneRepo.Setup(z => z.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Zone, bool>>>()))
					 .ReturnsAsync((Zone)null);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<TreeTypeDTO>(entity))
				  .Returns(new TreeTypeDTO { Id = 1, IsArchived = true });

			var handler = new EditTreeTypeIsArchivedCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			_treeRepo.Verify(r => r.Update(entity), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.IsTrue(result.Data.IsArchived);
		}
	}
}
