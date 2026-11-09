using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IAccountEmailService
{
    Task SendPasswordResetCodeAsync(CustomerAccount account, string code, CancellationToken cancellationToken = default);
}
