using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.BTP.Application.CQRS.TreeTypes;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using FluentValidation.TestHelper;
using AutoMapper;

namespace AP.BTP.Tests.Application.TreeTypes
{
	[TestClass]
	public class EditTreeTypeCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ITreeTypeRepository> _treeRepo;
		private EditTreeTypeCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_treeRepo = new Mock<ITreeTypeRepository>();

			_uow.SetupGet(x => x.TreeTypeRepository).Returns(_treeRepo.Object);

			_validator = new EditTreeTypeCommandValidator(_uow.Object);
		}

		private EditTreeTypeCommand Valid()
		{
			return new EditTreeTypeCommand
			{
				Id = 1,
				Name = "Eik",
				Description = "Sterke boom",
				ImageUrls = new() { "img1.png" },
				Instructions = new()
				{
					new EditTreeTypeCommand.InstructionInput
					{
						Season = "Lente",
						FileUrl = "/instr.pdf"
					}
				}
			};
		}

		// VALIDATOR

		[TestMethod]
		public async Task ValidCommand_ShouldPass()
		{
			_treeRepo.Setup(r => r.GetByName("Eik")).ReturnsAsync((TreeType)null);

			var result = await _validator.TestValidateAsync(Valid());

			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task InvalidId_ShouldFail()
		{
			var cmd = Valid();
			cmd.Id = 0;

			var result = await _validator.TestValidateAsync(cmd);

			result.ShouldHaveValidationErrorFor("Id");
		}

		[TestMethod]
		public async Task DuplicateName_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByName("Eik"))
					 .ReturnsAsync(new TreeType { Id = 99, Name = "Eik" });

			var result = await _validator.TestValidateAsync(Valid());

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

		// HANDLER

		[TestMethod]
		public async Task Handler_Updates_And_Commits()
		{
			var entity = new TreeType { Id = 1, Name = "Old" };

			_treeRepo.Setup(r => r.GetByIdWithIncludes(1)).ReturnsAsync(entity);

			var mapper = new Mock<IMapper>();
			mapper.Setup(m => m.Map<TreeTypeDTO>(entity))
				  .Returns(new TreeTypeDTO { Id = 1, Name = "Eik" });

			var handler = new EditTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			_treeRepo.Verify(r => r.Update(entity), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual("Eik", result.Data.Name);
		}

		[TestMethod]
		public async Task Handler_NotFound_ShouldFail()
		{
			_treeRepo.Setup(r => r.GetByIdWithIncludes(1)).ReturnsAsync((TreeType)null);

			var mapper = new Mock<IMapper>();

			var handler = new EditTreeTypeCommandHandler(_uow.Object, mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
			_treeRepo.Verify(r => r.Update(It.IsAny<TreeType>()), Times.Never);
			_uow.Verify(u => u.Commit(), Times.Never);
		}
	}
}
