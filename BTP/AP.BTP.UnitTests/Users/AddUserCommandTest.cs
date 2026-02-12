using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AP.BTP.Application.CQRS.Users;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Unittests.Users
{
	[TestClass]
	public class AddUserCommandTest
	{
		[TestMethod]
		public async Task Handler_Returns_Null_When_Duplicate()
		{
			var uowMock = new Mock<IUnitofWork>();
			var userRepo = new Mock<IUserRepository>();
			var mapperMock = new Mock<IMapper>();
			var emailMock = new Mock<IEmailSender>();

			uowMock.Setup(u => u.UserRepository).Returns(userRepo.Object);

			userRepo
				.Setup(r => r.GetByAuthId("auth-1"))
				.ReturnsAsync(new User { Id = 1, AuthId = "auth-1" });

			var handler = new AddUserCommandHandler(
				uowMock.Object,
				mapperMock.Object,
				emailMock.Object
			);

			var result = await handler.Handle(
				new AddUserCommand
				{
					User = new UserDTO
					{
						AuthId = "auth-1",
						Email = "a@b.com",
						Username = "x",
						Roles = new()
					}
				},
				CancellationToken.None
			);

			Assert.IsNull(result.Data);
		}

		[TestMethod]
		public async Task Handler_Creates_User_Sends_Email_And_Commits()
		{
			var uowMock = new Mock<IUnitofWork>();
			var userRepo = new Mock<IUserRepository>();
			var mapperMock = new Mock<IMapper>();
			var emailMock = new Mock<IEmailSender>();

			uowMock.Setup(u => u.UserRepository).Returns(userRepo.Object);

			userRepo
				.Setup(r => r.GetByAuthId("auth-2"))
				.ReturnsAsync((User)null);

			userRepo
				.Setup(r => r.Create(It.IsAny<User>()))
				.ReturnsAsync((User u) => u);

			userRepo
				.Setup(r => r.GetByRole(Role.Admin, It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new List<User>
				{
					new User { Email = "admin@test.com" }
				});

			mapperMock
				.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
				.Returns(new UserDTO
				{
					Id = 2,
					AuthId = "auth-2",
					Email = "c@d.com",
					Username = "y"
				});

			var emailDir = Path.Combine(
				Directory.GetCurrentDirectory(),
				"EmailTemplates"
			);

			Directory.CreateDirectory(emailDir);

			var templatePath = Path.Combine(emailDir, "RegisterRoleEmail.html");

			File.WriteAllText(templatePath,
				@"<html>
				Hello {{DisplayName}} ({{Email}})
				</html>");

			var handler = new AddUserCommandHandler(
				uowMock.Object,
				mapperMock.Object,
				emailMock.Object
			);

			var result = await handler.Handle(
				new AddUserCommand
				{
					User = new UserDTO
					{
						AuthId = "auth-2",
						Email = "c@d.com",
						Username = "y",
						Roles = new()
					}
				},
				CancellationToken.None
			);

			uowMock.Verify(u => u.Commit(), Times.Once);
			emailMock.Verify(
				e => e.SendEmail(It.IsAny<AP.BTP.Application.Models.EmailMessage>()),
				Times.Once
			);

			Assert.IsTrue(result.Succeeded);
			Assert.AreEqual("auth-2", result.Data.AuthId);
		}
	}
}
