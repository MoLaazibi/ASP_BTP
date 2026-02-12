using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentValidation.TestHelper;
using AP.BTP.Application.CQRS.Categories;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace AP.BTP.UnitTests.Categories
{
	[TestClass]
	public class ArchiveCategoryCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ICategoryRepository> _repo;
		private ArchiveCategoryCommandValidator _validator;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_repo = new Mock<ICategoryRepository>();

			_uow.SetupGet(u => u.CategoryRepository).Returns(_repo.Object);

			_validator = new ArchiveCategoryCommandValidator(_uow.Object);
		}

		[TestMethod]
		public async Task Validator_IdZero_Fails()
		{
			var cmd = new ArchiveCategoryCommand { CategoryId = 0 };
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor(x => x.CategoryId);
		}

		[TestMethod]
		public async Task Validator_CategoryNotFound_Fails()
		{
			_repo.Setup(r => r.GetById(10)).ReturnsAsync((Category)null);
			var cmd = new ArchiveCategoryCommand { CategoryId = 10 };
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldHaveValidationErrorFor(x => x.CategoryId);
		}

		[TestMethod]
		public async Task Validator_Valid_Passes()
		{
			_repo.Setup(r => r.GetById(5)).ReturnsAsync(new Category { Id = 5 });
			var cmd = new ArchiveCategoryCommand { CategoryId = 5 };
			var result = await _validator.TestValidateAsync(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[TestMethod]
		public async Task Handler_TogglesArchived_And_Commits()
		{
			var category = new Category
			{
				Id = 1,
				Name = "Test",
				Color = "#FFFFFF",
				IsArchived = false
			};

			_repo.Setup(r => r.GetById(1)).ReturnsAsync(category);
			_repo.Setup(r => r.Update(It.IsAny<Category>()))
				 .ReturnsAsync(category);

			var handler = new ArchiveCategoryCommandHandler(_uow.Object);

			var cmd = new ArchiveCategoryCommand { CategoryId = 1 };
			var result = await handler.Handle(cmd, CancellationToken.None);

			Assert.IsTrue(category.IsArchived);
			_repo.Verify(r => r.Update(category), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);
			Assert.IsTrue(result.Succeeded);
		}
	}
}
