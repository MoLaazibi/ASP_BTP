using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class AddNurserySiteCommandValidatorTest
	{
		private Mock<IUnitofWork> _uowMock;
		private AddNurserySiteCommandValidator _validator;
		private Mock<INurserySiteRepository> _nurseryRepo;
		private Mock<IAddressRepository> _addressRepo;
		private Mock<IZoneRepository> _zoneRepo;
		private Mock<ITreeTypeRepository> _treeTypeRepo;


		[TestInitialize]
		public void Setup()
		{
			_uowMock = new Mock<IUnitofWork>();

			// Create needed mock repositories
			var nurseryRepo = new Mock<INurserySiteRepository>();
			var addrRepo = new Mock<IAddressRepository>();
			var zoneRepo = new Mock<IZoneRepository>();
			var treeTypeRepo = new Mock<ITreeTypeRepository>();

			// Hook repositories into unit of work
			_uowMock.SetupGet(x => x.NurserySiteRepository).Returns(nurseryRepo.Object);
			_uowMock.SetupGet(x => x.AddressRepository).Returns(addrRepo.Object);
			_uowMock.SetupGet(x => x.ZoneRepository).Returns(zoneRepo.Object);
			_uowMock.SetupGet(x => x.TreeTypeRepository).Returns(treeTypeRepo.Object);

			// Save them so tests can configure behavior later
			_nurseryRepo = nurseryRepo;
			_addressRepo = addrRepo;
			_zoneRepo = zoneRepo;
			_treeTypeRepo = treeTypeRepo;

			_validator = new AddNurserySiteCommandValidator(_uowMock.Object);
		}


		private AddNurserySiteCommand CreateValidCommand()
		{
			return new AddNurserySiteCommand
			{
				NurserySite = new NurserySiteDTO
				{
					Id = 0,
					Name = "TestSite",
					AddressId = 0,
					GroundPlan = new GroundPlanDTO
					{
						Id = 0,
						FileUrl = "/files/gp.png",
						UploadTime = DateTime.UtcNow
					}
				},
				Address = new AddressDTO
				{
					Id = 0,
					StreetName = "Main",
					HouseNumber = "10",
					PostalCode = "1000"
				},
				GroundPlan = new GroundPlanDTO
				{
					Id = 0,
					FileUrl = "/files/gp.png",
					UploadTime = DateTime.UtcNow
				},
				UserId = 123,
				Zones = new()
				{
					new ZoneDTO
					{
						Id = 0,
						Code = "A1",
						Size = 10,
						TreeTypeId = 1
					}
				}
			};
		}

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			_uowMock.Setup(u => u.NurserySiteRepository.FindAsync(It.IsAny<Expression<Func<NurserySite, bool>>>()))
				.ReturnsAsync((NurserySite)null);

			_uowMock.Setup(u => u.AddressRepository.FindAsync(It.IsAny<Expression<Func<Address, bool>>>()))
				.ReturnsAsync((Address)null);

			var cmd = CreateValidCommand();

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task MissingName_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.NurserySite.Name = "";

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("NurserySite.Name");
		}

		[TestMethod]
		public async Task MissingAddress_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.Address = null!;

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Address");
		}

		[TestMethod]
		public async Task MissingGroundPlan_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.GroundPlan = null!;

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("GroundPlan");
		}

		[TestMethod]
		public async Task DuplicateUser_ShouldFail()
		{
			var cmd = CreateValidCommand();

			_uowMock.Setup(u => u.NurserySiteRepository.FindAsync(It.IsAny<Expression<Func<NurserySite, bool>>>()))
				.ReturnsAsync(new NurserySite { Id = 99, UserId = cmd.UserId });

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("UserId");
		}

		[TestMethod]
		public async Task DuplicateAddressOnOtherSite_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.NurserySite.Id = 1;

			_uowMock.Setup(u => u.AddressRepository.FindAsync(It.IsAny<Expression<Func<Address, bool>>>()))
				.ReturnsAsync(new Address { Id = 10 });

			_uowMock.Setup(u => u.NurserySiteRepository.GetById(cmd.NurserySite.Id))
				.ReturnsAsync((NurserySite)null);

			var result = await _validator.TestValidateAsync(cmd);

			Assert.IsTrue(result.Errors.Any(), "Expected validation errors but found none.");
			Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("Er bestaat al een andere kweeksite")),
				"Expected duplicate address error but it was not found.");
		}

		[TestMethod]
		public async Task Zone_NoCode_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.Zones[0].Code = "";

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Zones[0].Code");
		}

		[TestMethod]
		public async Task Zone_SizeZero_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.Zones[0].Size = 0;

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Zones[0].Size");
		}

		[TestMethod]
		public async Task Zone_InvalidTreeType_ShouldFail()
		{
			var cmd = CreateValidCommand();
			cmd.Zones[0].TreeTypeId = 0;

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Zones[0].TreeTypeId");
		}
	}
}
