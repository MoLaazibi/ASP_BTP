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
	public class AddCategoryCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ICategoryRepository> _repo;
		private Mock<IMapper> _mapper;
		private AddCategoryCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_repo = new Mock<ICategoryRepository>();
			_mapper = new Mock<IMapper>();

			_uow.SetupGet(u => u.CategoryRepository).Returns(_repo.Object);

			_validator = new AddCategoryCommandValidator(_uow.Object);
		}

		private AddCategoryCommand Valid()
		{
			return new AddCategoryCommand
			{
				Name = "TestCat",
				Color = "#FF0000"
			};
		}

		[TestMethod]
		public async Task Validator_ValidCommand_Passes()
		{
			var cmd = Valid();
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
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
			cmd.Color = "red";

			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor("Color");
		}

		[TestMethod]
		public async Task Handler_CreatesCategory_And_Commits()
		{
			var entity = new Category
			{
				Id = 1,
				Name = "TestCat",
				Color = "#FF0000"
			};

			_repo.Setup(r => r.Create(It.IsAny<Category>()))
				 .ReturnsAsync(entity);

			_mapper.Setup(m => m.Map<CategoryDTO>(It.IsAny<Category>()))
				   .Returns(new CategoryDTO
				   {
					   Id = 1,
					   Name = "TestCat",
					   Color = "#FF0000"
				   });

			var handler = new AddCategoryCommandHandler(
				_uow.Object, _mapper.Object);

			var result = await handler.Handle(Valid(), CancellationToken.None);

			_repo.Verify(r => r.Create(It.IsAny<Category>()), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);
			Assert.IsTrue(result.Succeeded);
		}
	}
}
