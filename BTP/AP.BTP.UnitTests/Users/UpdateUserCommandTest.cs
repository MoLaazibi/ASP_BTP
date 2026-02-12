using Moq;
using AP.BTP.Application.CQRS.Users;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;

namespace Unittests.Users
{
	[TestClass]
	public class UpdateUserCommandTest
	{
		[TestMethod]
		public async Task Handler_Throws_When_Not_Found()
		{
			var uowMock = new Mock<IUnitofWork>();
			var userRepo = new Mock<IUserRepository>();
			var mapperMock = new Mock<IMapper>();
			var emailMock = new Mock<IEmailSender>();

			uowMock.Setup(u => u.UserRepository).Returns(userRepo.Object);
			userRepo.Setup(r => r.GetByEmail("e@f.com"))
					.ReturnsAsync((User)null);

			var handler = new UpdateUserCommandHandler(
				uowMock.Object,
				mapperMock.Object,
				emailMock.Object
			);

			await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () =>
			{
				await handler.Handle(
					new UpdateUserCommand
					{
						User = new UserDTO
						{
							Email = "e@f.com",
							AuthId = "auth",
							Roles = new List<Role>()
						}
					},
					CancellationToken.None
				);
			});
		}

		[TestMethod]
		public async Task Handler_Updates_And_Sends_Email()
		{
			var uowMock = new Mock<IUnitofWork>();
			var userRepo = new Mock<IUserRepository>();
			var mapperMock = new Mock<IMapper>();
			var emailMock = new Mock<IEmailSender>();

			uowMock.Setup(u => u.UserRepository).Returns(userRepo.Object);

			var user = new User
			{
				Id = 3,
				Email = "g@h.com",
				AuthId = "old",
				Roles = new List<UserRole>()
			};

			userRepo.Setup(r => r.GetByEmail("g@h.com"))
					.ReturnsAsync(user);

			mapperMock.Setup(m => m.Map<UserDTO>(user))
					  .Returns(new UserDTO
					  {
						  Id = 3,
						  Email = "g@h.com",
						  AuthId = "new"
					  });

			var emailDir = Path.Combine(
				Directory.GetCurrentDirectory(),
				"EmailTemplates"
			);

			Directory.CreateDirectory(emailDir);

			var templatePath = Path.Combine(
				emailDir,
				"RoleRecievedEmail.html"
			);

			File.WriteAllText(templatePath,
				@"<html>
				Hello {{DisplayName}}
				Roles: {{Roles}}
				</html>");

			var handler = new UpdateUserCommandHandler(
				uowMock.Object,
				mapperMock.Object,
				emailMock.Object
			);

			var result = await handler.Handle(
				new UpdateUserCommand
				{
					User = new UserDTO
					{
						Email = "g@h.com",
						AuthId = "new",
						Roles = new List<Role> { Role.Admin }
					}
				},
				CancellationToken.None
			);

			uowMock.Verify(u => u.Commit(), Times.Once);
			emailMock.Verify(
				e => e.SendEmail(It.IsAny<AP.BTP.Application.Models.EmailMessage>()),
				Times.Once
			);

			Assert.IsNotNull(result);
			Assert.AreEqual("new", result.AuthId);
		}
	}
}
