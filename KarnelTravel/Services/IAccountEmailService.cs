namespace KarnelTravel.Services;

public interface IAccountEmailService
{
    Task<bool> SendPasswordResetCodeAsync(string email, string recipientName, string code, CancellationToken cancellationToken = default);
}
