using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface IWebsiteContentService
{
    Task<WebsiteContent> GetAsync(CancellationToken cancellationToken = default);
}
