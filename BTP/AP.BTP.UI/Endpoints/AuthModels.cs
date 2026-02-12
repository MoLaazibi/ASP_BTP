using AP.BTP.Domain;

namespace AP.BTP.UI.Endpoints
{
    public record LoginRequest(string Email, string Password);
    public record ApiLoginResult(APIUser? User, string Token, string AccessToken);
    public record APIUser(string Id, string Email, string? Name, string? Picture, List<Role>? Roles);
    public record Auth0ErrorResponse(string? ErrorDescription, string? Message);
}
