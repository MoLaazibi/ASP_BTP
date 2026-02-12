using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using Moq;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class AddZoneCommandHandlerTest
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IZoneRepository> _zoneRepoMock;
		private Mock<IMapper> _mapperMock;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_zoneRepoMock = new Mock<IZoneRepository>();
			_mapperMock = new Mock<IMapper>();

			_uowMock.SetupGet(u => u.ZoneRepository).Returns(_zoneRepoMock.Object);
		}

		private AddZoneCommand CreateValidCommand()
		{
			return new AddZoneCommand
			{
				Zone = new ZoneDTO
				{
					Code = "Z1",
					Size = 10,
					TreeTypeId = 5,
					NurserySiteId = 3
				}
			};
		}

		[TestMethod]
		public async Task Handler_Creates_Zone_And_Commits()
		{
			var cmd = CreateValidCommand();
			var handler = new AddZoneCommandHandler(_uowMock.Object, _mapperMock.Object);

			_zoneRepoMock.Setup(r => r.Create(It.IsAny<Zone>()))
				.ReturnsAsync((Zone z) => z);

			var result = await handler.Handle(cmd, CancellationToken.None);

			_zoneRepoMock.Verify(r => r.Create(It.IsAny<Zone>()), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);
			Assert.IsTrue(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_Creates_Zone_With_Correct_Data()
		{
			var cmd = CreateValidCommand();
			Zone createdZone = null!;
			var handler = new AddZoneCommandHandler(_uowMock.Object, _mapperMock.Object);

			_zoneRepoMock.Setup(r => r.Create(It.IsAny<Zone>()))
				.ReturnsAsync((Zone z) =>
				{
					createdZone = z; 
					return z;
				});

			await handler.Handle(cmd, CancellationToken.None);

			Assert.AreEqual(cmd.Zone.Code, createdZone.Code);
			Assert.AreEqual(cmd.Zone.Size, createdZone.Size);
			Assert.AreEqual(cmd.Zone.TreeTypeId, createdZone.TreeTypeId);
			Assert.AreEqual(cmd.Zone.NurserySiteId, createdZone.NurserySiteId);
		}

		[TestMethod]
		public async Task Handler_Returns_Mapped_DTO()
		{
			var cmd = CreateValidCommand();
			var handler = new AddZoneCommandHandler(_uowMock.Object, _mapperMock.Object);

			_zoneRepoMock.Setup(r => r.Create(It.IsAny<Zone>()))
				.ReturnsAsync(new Zone { Id = 10, Code = "Z1" });

			_mapperMock.Setup(m => m.Map<ZoneDTO>(It.IsAny<Zone>()))
				.Returns(new ZoneDTO { Id = 10, Code = "Z1" });

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(10, result.Data.Id);
			Assert.AreEqual("Z1", result.Data.Code);
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public async Task Handler_Throws_When_Request_IsNull()
		{
			var handler = new AddZoneCommandHandler(_uowMock.Object, _mapperMock.Object);
			await handler.Handle(null!, CancellationToken.None);
		}
	}
}
