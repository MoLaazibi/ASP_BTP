namespace AP.BTP.Application.Exceptions
{
    public class SmtpServerException : Exception
    {
        public SmtpServerException(Exception innerException, string action)
            : base($"failed to send the {action} to the admin.", innerException)
        {

        }
    }
}
