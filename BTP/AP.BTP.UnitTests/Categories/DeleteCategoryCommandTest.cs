using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AP.BTP.Application.CQRS.Categories;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace AP.BTP.UnitTests.Categories
{
	[TestClass]
	public class DeleteCategoryCommandTest
	{
		private Mock<IUnitofWork> _uow;
		private Mock<ICategoryRepository> _repo;
		private Mock<IMapper> _mapper;

		[TestInitialize]
		public void Setup()
		{
			_uow = new Mock<IUnitofWork>();
			_repo = new Mock<ICategoryRepository>();
			_mapper = new Mock<IMapper>();

			_uow.SetupGet(u => u.CategoryRepository).Returns(_repo.Object);
		}

		[TestMethod]
		public async Task Handler_DeletesCategory_And_Commits()
		{
			var existing = new Category
			{
				Id = 1,
				Name = "Test",
				Color = "#FFFFFF"
			};

			_repo.Setup(r => r.GetById(1)).ReturnsAsync(existing);
			_repo.Setup(r => r.Delete(existing)).Returns(Task.CompletedTask);
			_mapper.Setup(m => m.Map<CategoryDTO>(existing))
				   .Returns(new CategoryDTO { Id = 1, Name = "Test", Color = "#FFFFFF" });

			var handler = new DeleteCategoryCommandHandler(_uow.Object, _mapper.Object);

			var result = await handler.Handle(new DeleteCategoryCommand { Id = 1 }, CancellationToken.None);

			_repo.Verify(r => r.Delete(existing), Times.Once);
			_uow.Verify(u => u.Commit(), Times.Once);

			Assert.AreEqual(1, result.Id);
			Assert.AreEqual("Test", result.Name);
		}

		[TestMethod]
		public async Task Handler_CategoryNotFound_Throws()
		{
			_repo.Setup(r => r.GetById(999)).ReturnsAsync((Category)null);

			var handler = new DeleteCategoryCommandHandler(_uow.Object, _mapper.Object);

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
				handler.Handle(new DeleteCategoryCommand { Id = 999 }, CancellationToken.None));
		}

	}
}
