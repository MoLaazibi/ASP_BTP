using Moq;
using FluentValidation.TestHelper;
using System.Linq.Expressions;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using MediatR;
using AP.BTP.Application.CQRS;

namespace AP.BTP.Tests.Application.NurserySites
{
	[TestClass]
	public class EditNurserySiteCommandValidatorTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<INurserySiteRepository> _siteRepo;
		private Mock<IAddressRepository> _addressRepo;
		private EditNurserySiteCommandValidator _validator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_siteRepo = new Mock<INurserySiteRepository>();
			_addressRepo = new Mock<IAddressRepository>();

			_uow.SetupGet(x => x.NurserySiteRepository).Returns(_siteRepo.Object);
			_uow.SetupGet(x => x.AddressRepository).Returns(_addressRepo.Object);

			_validator = new EditNurserySiteCommandValidator(_uow.Object);
		}

		private EditNurserySiteCommand Valid()
		{
			return new EditNurserySiteCommand
			{
				NurserySite = new NurserySiteDTO
				{
					Id = 10,
					Name = "Site A",
					AddressId = 1,
					GroundPlan = new GroundPlanDTO { Id = 2, FileUrl = "/gp.png" }
				},
				Address = new AddressDTO
				{
					Id = 1,
					StreetName = "Main",
					HouseNumber = "10",
					PostalCode = "1000"
				},
				GroundPlan = new GroundPlanDTO { Id = 2, FileUrl = "/gp.png" },
				UserId = 5,
				Zones = new List<ZoneDTO>
				{
					new ZoneDTO { Id = 1, Code = "Z1", Size = 10, TreeTypeId = 5 }
				}
			};
		}

		[TestMethod]
		public async Task Valid_ShouldPass()
		{
			var cmd = Valid();

			_siteRepo.Setup(r => r.GetById(cmd.NurserySite.Id))
				.ReturnsAsync(new NurserySite { Id = cmd.NurserySite.Id });

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			var cmd = Valid();
			cmd.NurserySite.Id = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("NurserySite.Id");
		}

		[TestMethod]
		public async Task SiteNotFound_ShouldFail()
		{
			var cmd = Valid();

			_siteRepo.Setup(r => r.GetById(cmd.NurserySite.Id))
				.ReturnsAsync((NurserySite)null);

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("NurserySite.Id");
		}

		[TestMethod]
		public async Task NameEmpty_ShouldFail()
		{
			var cmd = Valid();
			cmd.NurserySite.Name = "";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("NurserySite.Name");
		}

		[TestMethod]
		public async Task AddressMissing_ShouldFail()
		{
			var cmd = Valid();
			cmd.Address = null;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Address");
		}

		[TestMethod]
		public async Task GroundPlanMissing_ShouldFail()
		{
			var cmd = Valid();
			cmd.GroundPlan = null;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("GroundPlan");
		}

		[TestMethod]
		public async Task ZoneMissingCode_ShouldFail()
		{
			var cmd = Valid();
			cmd.Zones[0].Code = "";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zones[0].Code");
		}

		[TestMethod]
		public async Task ZoneSizeZero_ShouldFail()
		{
			var cmd = Valid();
			cmd.Zones[0].Size = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zones[0].Size");
		}

		[TestMethod]
		public async Task ZoneTreeInvalid_ShouldFail()
		{
			var cmd = Valid();
			cmd.Zones[0].TreeTypeId = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Zones[0].TreeTypeId");
		}

		[TestMethod]
		public async Task DuplicateUser_ShouldFail()
		{
			var cmd = Valid();

			_siteRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<NurserySite, bool>>>()))
				.ReturnsAsync(new NurserySite { Id = 999 });

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("UserId");
		}

		[TestMethod]
		public async Task DuplicateAddress_ShouldFail()
		{
			var cmd = Valid();

			_addressRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Address, bool>>>()))
				.ReturnsAsync(new Address { Id = 55 });

			_siteRepo.Setup(r => r.GetById(cmd.NurserySite.Id))
				.ReturnsAsync((NurserySite)null);

			var result = await _validator.TestValidateAsync(cmd);
			Assert.IsTrue(result.Errors.Count > 0);
		}
	}

	[TestClass]
	public class EditNurserySiteCommandHandlerTests
	{
		private Mock<IUnitofWork> _uow;
		private Mock<INurserySiteRepository> _siteRepo;
		private Mock<IMapper> _mapper;
		private Mock<IMediator> _mediator;

		[TestInitialize]
		public void Init()
		{
			_uow = new Mock<IUnitofWork>();
			_siteRepo = new Mock<INurserySiteRepository>();
			_mapper = new Mock<IMapper>();
			_mediator = new Mock<IMediator>();

			_uow.SetupGet(x => x.NurserySiteRepository).Returns(_siteRepo.Object);
		}

		private EditNurserySiteCommand Valid()
		{
			return new EditNurserySiteCommand
			{
				NurserySite = new NurserySiteDTO { Id = 10, Name = "New", GroundPlan = new GroundPlanDTO { FileUrl = "/a.png" } },
				Address = new AddressDTO { StreetName = "S", HouseNumber = "1", PostalCode = "1000" },
				GroundPlan = new GroundPlanDTO { FileUrl = "/a.png" },
				UserId = 999,
				Zones = new List<ZoneDTO>
				{
					new ZoneDTO { Id = 0, Code = "NEW", Size = 10, TreeTypeId = 5 },
					new ZoneDTO { Id = 5, Code = "EDIT", Size = 20, TreeTypeId = 6 },
					new ZoneDTO { Id = 7, Code = "DEL", Size = 15, TreeTypeId = 1, IsDeleted = true }
				}
			};
		}

		private NurserySite Existing()
		{
			return new NurserySite
			{
				Id = 10,
				Name = "Old",
				UserId = 1,
				Address = new Address { StreetName = "A", HouseNumber = "1", PostalCode = "1000" },
				GroundPlan = new GroundPlan { FileUrl = "/old.png", UploadTime = DateTime.UtcNow },
				Zones = new List<Zone>()
			};
		}

		[TestMethod]
		public async Task NotFound_ShouldFail()
		{
			var cmd = Valid();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id))
				.ReturnsAsync((NurserySite)null);

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}

		[TestMethod]
		public async Task Updates_Name_And_User()
		{
			var cmd = Valid();
			var site = Existing();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id))
				.ReturnsAsync(site);

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site)).Returns(cmd.NurserySite);

			_mediator.Setup(m => m.Send(It.IsAny<IRequest<Result<ZoneDTO>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<ZoneDTO>.Success(new ZoneDTO(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual(cmd.NurserySite.Name, site.Name);
			Assert.AreEqual(cmd.UserId, site.UserId);
		}

		[TestMethod]
		public async Task Adds_New_Zones()
		{
			var cmd = Valid();
			cmd.Zones = cmd.Zones.Where(z => z.Id == 0).ToList();

			var site = Existing();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id)).ReturnsAsync(site);

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site)).Returns(cmd.NurserySite);

			_mediator.Setup(m => m.Send(It.IsAny<AddZoneCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<ZoneDTO>.Success(new ZoneDTO(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<AddZoneCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Edits_Existing_Zones()
		{
			var cmd = Valid();
			cmd.Zones = cmd.Zones.Where(z => z.Id == 5).ToList();

			var site = Existing();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id)).ReturnsAsync(site);

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site)).Returns(cmd.NurserySite);

			_mediator.Setup(m => m.Send(It.IsAny<EditZoneCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<Zone>.Success(new Zone(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<EditZoneCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Deletes_Zones()
		{
			var cmd = Valid();
			cmd.Zones = cmd.Zones.Where(z => z.IsDeleted).ToList();

			var site = Existing();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id)).ReturnsAsync(site);

			_mapper.Setup(m => m.Map<ZoneDTO>(It.IsAny<ZoneDTO>())).Returns((ZoneDTO z) => z);
			_mapper.Setup(m => m.Map<NurserySiteDTO>(site)).Returns(cmd.NurserySite);

			_mediator.Setup(m => m.Send(It.IsAny<DeleteZoneCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<ZoneDTO>.Success(new ZoneDTO(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_mediator.Verify(m => m.Send(It.IsAny<DeleteZoneCommand>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Commits_When_Updated()
		{
			var cmd = Valid();
			var site = Existing();
			var handler = new EditNurserySiteCommandHandler(_uow.Object, _mapper.Object, _mediator.Object);

			_siteRepo.Setup(r => r.GetByIdWithDetails(cmd.NurserySite.Id)).ReturnsAsync(site);

			_mapper.Setup(m => m.Map<NurserySiteDTO>(site)).Returns(cmd.NurserySite);

			_mediator.Setup(m => m.Send(It.IsAny<IRequest<Result<ZoneDTO>>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(Result<ZoneDTO>.Success(new ZoneDTO(), ""));

			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(result.Succeeded);
			_uow.Verify(u => u.Commit(), Times.Once);
		}
	}
}
