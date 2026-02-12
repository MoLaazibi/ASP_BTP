using AP.BTP.Application.Exceptions;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Diagnostics;

namespace AP.BTP.Application.CQRS.Users
{
    public class AddUserCommand : IRequest<Result<UserDTO>>
    {
        public required UserDTO User { get; set; }

    }


    public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
    {
        public AddUserCommandValidator(IUnitofWork uow)
        {
            RuleFor(u => u.User)
                .NotNull()
                .WithMessage("User object is verplicht.");

            RuleFor(u => u.User.AuthId)
                .NotEmpty().WithMessage("AuthId is verplicht.")
                .MustAsync(async (authId, ct) =>
                {
                    var existing = await uow.UserRepository.GetByAuthId(authId);
                    return existing == null;
                })
                .WithMessage("Er bestaat al een gebruiker met deze AuthId.");

            RuleFor(u => u.User.Email)
                .NotEmpty().WithMessage("Email is verplicht.")
                .EmailAddress().WithMessage("Email-formaat is ongeldig.")
                .MaximumLength(100).WithMessage("Email mag maximaal 150 tekens lang zijn.");

            RuleFor(u => u.User.Username)
                .NotEmpty().WithMessage("Username is verplicht.")
                .MaximumLength(50).WithMessage("Username mag maximaal 50 tekens lang zijn.");

            RuleFor(u => u.User.FirstName)
                .MaximumLength(100).WithMessage("Voornaam mag maximaal 100 tekens lang zijn.")
                .When(u => !string.IsNullOrWhiteSpace(u.User.FirstName));

            RuleFor(u => u.User.LastName)
                .MaximumLength(100).WithMessage("Achternaam mag maximaal 100 tekens lang zijn.")
                .When(u => !string.IsNullOrWhiteSpace(u.User.LastName));

            RuleFor(u => u.User.Roles)
                .NotNull().WithMessage("Roles mag niet null zijn.");
        }
    }

    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, Result<UserDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        public AddUserCommandHandler(IUnitofWork uow, IMapper mapper, IEmailSender emailSender)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.emailSender = emailSender;
        }

        public async Task<Result<UserDTO>> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                return Result<UserDTO>.Success(null, "User data is required");


            var existingUser = await uow.UserRepository.GetByAuthId(request.User.AuthId);
            if (existingUser != null)
                return Result<UserDTO>.Success(null, "User with this AuthId already exists");



            var newUser = new User
            {
                AuthId = request.User.AuthId,
                Email = request.User.Email,
                Username = request.User.Username,
                FirstName = request.User.FirstName,
                LastName = request.User.LastName,
                Roles = (request.User.Roles ?? new List<Role>())
                    .Select(r => new UserRole { Role = r })
                    .ToList()
            };

            await uow.UserRepository.Create(newUser);
            await uow.Commit();

            var dto = mapper.Map<UserDTO>(newUser);
            var displayName = (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
                ? $"{dto.FirstName} {dto.LastName}".Trim()
                : dto.Email;
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "RegisterRoleEmail.html");
                string template = await File.ReadAllTextAsync(templatePath);

                string htmlBody = template
                    .Replace("{{DisplayName}}", displayName)
                    .Replace("{{Email}}", dto.Email)
                    .Replace("{{RegistrationDate}}", DateTime.Now.ToString("dd/MM/yyyy"))
                    .Replace("{{Year}}", DateTime.Now.Year.ToString());

                var users = await uow.UserRepository.GetByRole(Role.Admin, 1, 10);
                foreach (var user in users)
                {
                    await emailSender.SendEmail(new Models.EmailMessage()
                    {
                        To = user.Email,
                        Subject = "User registration",
                        Body = htmlBody,
                        IsHtml = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SMTP error while sending registration email: {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                throw new SmtpServerException(ex.InnerException, "Registration email");
            }


            return Result<UserDTO>.Success(dto, "User successfully created");
        }
    }
}
