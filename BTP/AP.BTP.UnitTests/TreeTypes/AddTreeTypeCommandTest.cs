using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.TreeTypes;
using AP.BTP.Application.Interfaces;
using AP.BTP.Application.CQRS.Instructions;
using AP.BTP.Domain;
using FluentValidation.TestHelper;
using AutoMapper;

namespace AP.BTP.Tests.Application.TreeTypes
{
	[TestClass]
	public class AddTreeTypeCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITreeTypeRepository> _treeRepo;
		private AddTreeTypeCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_treeRepo = new Mock<ITreeTypeRepository>();

			_uow.SetupGet(x => x.TreeTypeRepository).Returns(_treeRepo.Object);

			_validator = new AddTreeTypeCommandValidator(_uow.Object);
		}

		private AddTreeTypeCommand Valid()
		{
			return new AddTreeTypeCommand
			{
				Name = "Eik",
				Description = "Sterke boom",
				ImageUrls = new() { "img1.png", "img2.png" },
				Instructions = new()
				{
					new AddTreeTypeCommand.InstructionInput
					{
						Season = "Lente",
						FileUrl = "/instr1.pdf"
					}
				},
				QrCodeUrl = "/qrcode.png"
			};
		}

		// VALIDATOR TESTS

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			_treeRepo.Setup(r => r.GetByName("Eik")).ReturnsAsync((TreeType)null);

			var result = await _validator.TestValidateAsync(Valid());

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task DuplicateName_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByName("Eik"))
					 .ReturnsAsync(new TreeType { Id = 5, Name = "Eik" });

			var cmd = Valid();

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Name");
		}

		[TestMethod]
		public async Task MissingName_ShouldFail()
		{
			var cmd = Valid();
			cmd.Name = "";

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Name");
		}

		[TestMethod]
		public async Task Instruction_MissingSeason_ShouldFail()
		{
			var cmd = Valid();
			cmd.Instructions[0].Season = "";

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Instructions[0].Season");
		}

		[TestMethod]
		public async Task Instruction_MissingFileUrl_ShouldFail()
		{
			var cmd = Valid();
			cmd.Instructions[0].FileUrl = "";

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Instructions[0].FileUrl");
		}

		// HANDLER TEST

		[TestMethod]
		public async Task Handler_CreatesTreeType_AndCommits()
		{
			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<TreeTypeDTO>(It.IsAny<TreeType>()))
				  .Returns(new TreeTypeDTO { Id = 1, Name = "Eik" });

			var handler = new AddTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var cmd = Valid();

			var result = await handler.Handle(cmd, CancellationToken.None);

			_treeRepo.Verify(r => r.Create(It.IsAny<TreeType>()), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual("Eik", result.Data.Name);
		}
	}
}
