using AP.BTP.Application.Models;

namespace AP.BTP.Application.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendEmail(EmailMessage email);
    }

}
