namespace AP.BTP.MobileUI.Services.Auth
{
    public record ApiLoginResult(ApiUser? User, string Token, string AccessToken);
    public record ApiUser(string Id, string Email, string? Name, string? Picture, List<AP.BTP.Domain.Role>? Roles);
}
