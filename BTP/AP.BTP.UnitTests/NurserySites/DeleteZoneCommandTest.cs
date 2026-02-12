using Moq;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class DeleteZoneCommandHandlerTest
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IZoneRepository> _zoneRepoMock;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_zoneRepoMock = new Mock<IZoneRepository>();

			_uowMock.SetupGet(u => u.ZoneRepository).Returns(_zoneRepoMock.Object);
		}

		private DeleteZoneCommand CreateValidCommand()
		{
			return new DeleteZoneCommand
			{
				Zone = new ZoneDTO
				{
					Id = 10,
					Code = "A1",
					Size = 20,
					TreeTypeId = 3,
					NurserySiteId = 5
				}
			};
		}

		[TestMethod]
		public async Task Handler_Deletes_Zone_And_Commits()
		{
			var cmd = CreateValidCommand();
			var handler = new DeleteZoneCommandHandler(_uowMock.Object);

			var zoneEntity = new Zone { Id = cmd.Zone.Id };

			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id))
				.ReturnsAsync(zoneEntity);

			var result = await handler.Handle(cmd, CancellationToken.None);

			_zoneRepoMock.Verify(r => r.GetById(cmd.Zone.Id), Times.Once);
			_zoneRepoMock.Verify(r => r.Delete(zoneEntity), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(cmd.Zone.Id, result.Data.Id);
			Assert.AreEqual("Zone succesvol verwijderd", result.Message);
		}

		[TestMethod]
		public async Task Handler_Returns_Failure_When_Zone_Not_Found()
		{
			var cmd = CreateValidCommand();
			var handler = new DeleteZoneCommandHandler(_uowMock.Object);

			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id))
				.ReturnsAsync((Zone)null);

			var result = await handler.Handle(cmd, CancellationToken.None);

			_zoneRepoMock.Verify(r => r.Delete(It.IsAny<Zone>()), Times.Never);
			_uowMock.Verify(u => u.Commit(), Times.Never);

			Assert.IsFalse(result.Succeeded);
			Assert.IsTrue(result.Message.Contains("niet gevonden"));
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public async Task Handler_Throws_When_Request_Zone_IsNull()
		{
			var handler = new DeleteZoneCommandHandler(_uowMock.Object);

			var cmd = new DeleteZoneCommand
			{
				Zone = null!
			};

			await handler.Handle(cmd, CancellationToken.None);
		}
	}
}
