using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    // Command
    public class UpdateUserCommand : IRequest<UserDTO>
    {
        public required UserDTO User { get; set; }
    }

    // Validator
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(IUnitofWork uow)
        {
            // User object moet bestaan
            RuleFor(x => x.User)
                .NotNull()
                .WithMessage("User object mag niet null zijn.");

            When(x => x.User != null, () =>
            {
                RuleFor(x => x.User.Email)
                    .NotEmpty().WithMessage("Email mag niet leeg zijn.")
                    .EmailAddress().WithMessage("Ongeldig email-formaat.")
                    .MaximumLength(100).WithMessage("Email mag maximaal 150 karakters lang zijn.");

                RuleFor(x => x.User.AuthId)
                    .NotEmpty().WithMessage("AuthId mag niet leeg zijn.");

                RuleFor(x => x.User.Username)
                    .NotEmpty().WithMessage("Username is verplicht.")
                    .MaximumLength(50).WithMessage("Username mag maximaal 50 karakters lang zijn.");

                RuleFor(x => x.User.FirstName)
                    .MaximumLength(100).WithMessage("Voornaam mag maximaal 100 karakters lang zijn.")
                    .When(x => !string.IsNullOrWhiteSpace(x.User.FirstName));

                RuleFor(x => x.User.LastName)
                    .MaximumLength(100).WithMessage("Achternaam mag maximaal 100 karakters lang zijn.")
                    .When(x => !string.IsNullOrWhiteSpace(x.User.LastName));

                RuleFor(x => x.User.AuthId)
                    .MustAsync(async (authId, ct) => await uow.UserRepository.GetByAuthId(authId) != null)
                    .WithMessage("Gebruiker werd niet gevonden.");

                RuleFor(x => x)
                    .MustAsync(async (cmd, ct) =>
                    {
                        var existing = await uow.UserRepository.GetByEmail(cmd.User.Email);
                        return existing == null || existing.AuthId == cmd.User.AuthId;
                    })
                    .WithMessage("Dit emailadres is al in gebruik door een andere gebruiker.");
            });
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDTO>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        public UpdateUserCommandHandler(IUnitofWork uow, IMapper mapper, IEmailSender emailSender)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.emailSender = emailSender;
        }

        public async Task<UserDTO> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var existing = await uow.UserRepository.GetByEmail(request.User.Email);

            if (existing == null)
            {
                throw new KeyNotFoundException("Gebruiker niet gevonden");
            }

            existing.AuthId = request.User.AuthId;
            existing.FirstName = request.User.FirstName;
            existing.LastName = request.User.LastName;
            existing.PreferredNurserySiteId = request.User.PreferredNurserySiteId;
            existing.Roles = (request.User.Roles ?? new List<Role>())
                .Select(r => new UserRole { Role = r, UserId = existing.Id })
                .ToList();

            await uow.Commit();

            var displayName = (!string.IsNullOrWhiteSpace(existing.FirstName) || !string.IsNullOrWhiteSpace(existing.LastName))
                ? $"{existing.FirstName} {existing.LastName}".Trim()
                : existing.Email;

            string roles = string.Join(", ", existing.Roles.Select(r => r.Role.ToString()));


            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "RoleRecievedEmail.html");
            string template = await File.ReadAllTextAsync(templatePath);

            string htmlBody = template
                .Replace("{{DisplayName}}", displayName)
                .Replace("{{Roles}}", roles)
                .Replace("{{AssignmentDate}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{Year}}", DateTime.Now.Year.ToString());

            await emailSender.SendEmail(new Models.EmailMessage()
            {
                To = existing.Email,
                Subject = "Rolen toegewezen",
                Body = htmlBody,
                IsHtml = true
            });

            return mapper.Map<UserDTO>(existing);
        }
    }
}
