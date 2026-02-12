using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.Categories;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace AP.BTP.Tests.Application.Categories
{
	[TestClass]
	public class EditCategoryCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ICategoryRepository> _repo;
		private Mock<IMapper> _mapper;
		private EditCategoryCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_repo = new Mock<ICategoryRepository>();
			_mapper = new Mock<IMapper>();

			_uow.SetupGet(u => u.CategoryRepository).Returns(_repo.Object);

			_validator = new EditCategoryCommandValidator();
		}

		private EditCategoryCommand Valid()
		{
			return new EditCategoryCommand
			{
				Id = 1,
				Name = "Updated",
				Color = "#123456"
			};
		}

		[TestMethod]
		public async Task Validator_ValidCommand_Passes()
		{
			var result = await _validator.TestValidateAsync(Valid());
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task Validator_InvalidId_Fails()
		{
			var cmd = Valid();
			cmd.Id = 0;

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Id");
		}

		[TestMethod]
		public async Task Validator_MissingName_Fails()
		{
			var cmd = Valid();
			cmd.Name = "";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Name");
		}

		[TestMethod]
		public async Task Validator_InvalidHexColor_Fails()
		{
			var cmd = Valid();
			cmd.Color = "blue";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Color");
		}

		[TestMethod]
		public async Task Handler_UpdatesCategory_And_Commits()
		{
			var existing = new Category
			{
				Id = 1,
				Name = "Old",
				Color = "#FFFFFF"
			};

			_repo.Setup(r => r.GetById(1))
				 .ReturnsAsync(existing);

			_repo.Setup(r => r.Update(It.IsAny<Category>()))
				 .ReturnsAsync(existing);


			_mapper.Setup(m => m.Map<CategoryDTO>(existing))
				   .Returns(new CategoryDTO
				   {
					   Id = 1,
					   Name = "Updated",
					   Color = "#123456"
				   });

			var handler = new EditCategoryCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			_repo.Verify(r => r.Update(existing), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);
			Assert.IsTrue(result.Succeeded);
		}

		[TestMethod]
		public async Task Handler_CategoryNotFound_Fails()
		{
			_repo.Setup(r => r.GetById(1))
				 .ReturnsAsync((Category)null);

			var handler = new EditCategoryCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			Assert.IsFalse(result.Succeeded);
		}
	}
}
