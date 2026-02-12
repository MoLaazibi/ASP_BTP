using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class EditZoneCommandValidatorTest
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IZoneRepository> _zoneRepoMock;
		private EditZoneCommandHandler.EditZoneCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_zoneRepoMock = new Mock<IZoneRepository>();
			_uowMock.SetupGet(u => u.ZoneRepository).Returns(_zoneRepoMock.Object);

			_validator = new EditZoneCommandHandler.EditZoneCommandValidator(_uowMock.Object);
		}

		private EditZoneCommand CreateValid()
		{
			return new EditZoneCommand
			{
				Zone = new ZoneDTO
				{
					Id = 10,
					Code = "A1",
					Size = 20,
					TreeTypeId = 5,
					NurserySiteId = 3
				}
			};
		}

		[TestMethod]
		public async Task Valid_ShouldPass()
		{
			var cmd = CreateValid();
			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id)).ReturnsAsync(new Zone { Id = cmd.Zone.Id });
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task ZoneNull_ShouldFail()
		{
			var cmd = new EditZoneCommand { Zone = null };
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone");
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			var cmd = CreateValid();
			cmd.Zone.Id = 0;
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.Id");
		}

		[TestMethod]
		public async Task ZoneNotFound_ShouldFail()
		{
			var cmd = CreateValid();
			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id)).ReturnsAsync((Zone)null);
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.Id");
		}

		[TestMethod]
		public async Task MissingCode_ShouldFail()
		{
			var cmd = CreateValid();
			cmd.Zone.Code = "";
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.Code");
		}

		[TestMethod]
		public async Task SizeZero_ShouldFail()
		{
			var cmd = CreateValid();
			cmd.Zone.Size = 0;
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.Size");
		}

		[TestMethod]
		public async Task InvalidTreeType_ShouldFail()
		{
			var cmd = CreateValid();
			cmd.Zone.TreeTypeId = 0;
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.TreeTypeId");
		}

		[TestMethod]
		public async Task InvalidNurserySiteId_ShouldFail()
		{
			var cmd = CreateValid();
			cmd.Zone.NurserySiteId = 0;
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zone.NurserySiteId");
		}
	}

	[TestClass]
	public class EditZoneCommandHandlerTests
	{
		private Mock<IUnitofWork> _uowMock;
		private Mock<IZoneRepository> _zoneRepoMock;
		private Mock<ITreeTypeRepository> _treeRepoMock;

		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();
			_zoneRepoMock = new Mock<IZoneRepository>();
			_treeRepoMock = new Mock<ITreeTypeRepository>();

			_uowMock.SetupGet(u => u.ZoneRepository).Returns(_zoneRepoMock.Object);
			_uowMock.SetupGet(u => u.TreeTypeRepository).Returns(_treeRepoMock.Object);
		}

		private EditZoneCommand CreateValid()
		{
			return new EditZoneCommand
			{
				Zone = new ZoneDTO
				{
					Id = 10,
					Code = "Z1",
					Size = 12,
					TreeTypeId = 5,
					NurserySiteId = 3
				}
			};
		}

		[TestMethod]
		public async Task Updates_Zone_And_Commits()
		{
			var cmd = CreateValid();
			var handler = new EditZoneCommandHandler(_uowMock.Object);

			var existing = new Zone { Id = 10, Code = "OLD", Size = 50, TreeTypeId = 1 };
			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id)).ReturnsAsync(existing);
			_treeRepoMock.Setup(r => r.GetById(cmd.Zone.TreeTypeId)).ReturnsAsync(new TreeType { Id = 5, Name = "Oak" });

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(cmd.Zone.Code, existing.Code);
			Assert.AreEqual(cmd.Zone.Size, existing.Size);
			Assert.AreEqual(cmd.Zone.TreeTypeId, existing.TreeTypeId);

			_zoneRepoMock.Verify(r => r.Update(existing), Times.Once);
			_uowMock.Verify(u => u.Commit(), Times.Once);
		}

		[TestMethod]
		public async Task Fails_When_Zone_NotFound()
		{
			var cmd = CreateValid();
			var handler = new EditZoneCommandHandler(_uowMock.Object);

			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id)).ReturnsAsync((Zone)null);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Fails_When_TreeType_NotFound()
		{
			var cmd = CreateValid();
			var handler = new EditZoneCommandHandler(_uowMock.Object);

			_zoneRepoMock.Setup(r => r.GetById(cmd.Zone.Id)).ReturnsAsync(new Zone { Id = 10 });
			_treeRepoMock.Setup(r => r.GetById(cmd.Zone.TreeTypeId)).ReturnsAsync((TreeType)null);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		[ExpectedException(typeof(System.NullReferenceException))]
		public async Task Throws_When_Zone_Request_IsNull()
		{
			var handler = new EditZoneCommandHandler(_uowMock.Object);
			var cmd = new EditZoneCommand { Zone = null };
			await handler.Handle(cmd, CancellationToken.None);
		}
	}
}
