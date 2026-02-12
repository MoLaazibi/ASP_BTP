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
	public class UpdateTreeTypeQrCodeCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITreeTypeRepository> _treeRepo;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_treeRepo = new Mock<ITreeTypeRepository>();

			_uow.SetupGet(u => u.TreeTypeRepository).Returns(_treeRepo.Object);
		}

		[TestMethod]
		public async Task Handler_TreeTypeNotFound_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetById(5)).ReturnsAsync((TreeType)null);

			var mapper = new Mock<IMapper>();
			var handler = new UpdateTreeTypeQrCodeCommandHandler(_uow.Object, mapper.Object);

			var cmd = new UpdateTreeTypeQrCodeCommand
			{
				TreeTypeId = 5,
				QrCodeUrl = "/qr/new.png"
			};

			try
			{
				await handler.Handle(cmd, CancellationToken.None);
				Assert.Fail("Expected exception due to null entity.");
			}
			catch
			{
				Assert.IsTrue(true);
			}

			_uow.Verify(u => u.Commit(), Times.Never);
		}

		[TestMethod]
		public async Task Handler_UpdatesQrCode_And_Commits()
		{
			var entity = new TreeType { Id = 5, QrCodeUrl = null };

			_treeRepo.Setup(r => r.GetById(5)).ReturnsAsync(entity);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<TreeTypeDTO>(entity))
				  .Returns(new TreeTypeDTO { Id = 5, QrCodeUrl = "/qr/new.png" });

			var handler = new UpdateTreeTypeQrCodeCommandHandler(_uow.Object, mapper.Object);

			var cmd = new UpdateTreeTypeQrCodeCommand
			{
				TreeTypeId = 5,
				QrCodeUrl = "/qr/new.png"
			};

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual("/qr/new.png", entity.QrCodeUrl);
			Assert.AreEqual("/qr/new.png", result.Data.QrCodeUrl);

			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
