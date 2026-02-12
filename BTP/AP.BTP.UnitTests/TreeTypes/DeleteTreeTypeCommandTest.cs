using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.TreeTypes;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;

namespace AP.BTP.Tests.Application.TreeTypes
{
	[TestClass]
	public class DeleteTreeTypeCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITreeTypeRepository> _treeRepo;
		private Mock<IZoneRepository> _zoneRepo;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_treeRepo = new Mock<ITreeTypeRepository>();
			_zoneRepo = new Mock<IZoneRepository>();

			_uow.SetupGet(x => x.TreeTypeRepository).Returns(_treeRepo.Object);
			_uow.SetupGet(x => x.ZoneRepository).Returns(_zoneRepo.Object);
		}

		[TestMethod]
		public async Task Handler_NotFound_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByIdWithIncludes(5)).ReturnsAsync((TreeType)null);

			var mapper = new Mock<IMapper>();
			var handler = new DeleteTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(new DeleteTreeTypeCommand { Id = 5 }, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_NotArchived_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByIdWithIncludes(5))
					 .ReturnsAsync(new TreeType { Id = 5, IsArchived = false });

			var mapper = new Mock<IMapper>();
			var handler = new DeleteTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(new DeleteTreeTypeCommand { Id = 5 }, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_ZoneUsesTreeType_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByIdWithIncludes(5))
					 .ReturnsAsync(new TreeType { Id = 5, IsArchived = true });

			_zoneRepo.Setup(z => z.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Zone, bool>>>()))
					 .ReturnsAsync(new Zone { Id = 99 });

			var mapper = new Mock<IMapper>();
			var handler = new DeleteTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(new DeleteTreeTypeCommand { Id = 5 }, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_Deletes_And_Commits()
		{
			var entity = new TreeType { Id = 5, IsArchived = true };

			_treeRepo.Setup(r => r.GetByIdWithIncludes(5)).ReturnsAsync(entity);
			_zoneRepo.Setup(z => z.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Zone, bool>>>()))
					 .ReturnsAsync((Zone)null);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<TreeTypeDTO>(entity))
				  .Returns(new TreeTypeDTO { Id = 5 });

			var handler = new DeleteTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(new DeleteTreeTypeCommand { Id = 5 }, CancellationToken.None);

			_treeRepo.Verify(r => r.Delete(entity), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(5, result.Data.Id);
		}
	}
}
