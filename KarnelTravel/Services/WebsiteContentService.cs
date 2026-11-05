using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Services;

public sealed class WebsiteContentService : IWebsiteContentService
{
    private readonly ApplicationDbContext _context;
    private Task<WebsiteContent>? _contentTask;

    public WebsiteContentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<WebsiteContent> GetAsync(CancellationToken cancellationToken = default)
    {
        return _contentTask ??= LoadAsync(cancellationToken);
    }

    private async Task<WebsiteContent> LoadAsync(CancellationToken cancellationToken)
    {
        var content = await _context.WebsiteContents
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return content ?? new WebsiteContent
        {
            SiteName = "Karnel Travel Guide",
            Tagline = "Discover Pakistan, differently.",
            HomeHeroHeading = "Go where the light feels different.",
            HomeHeroSubtitle = "Thoughtful journeys across Pakistan, shaped by people who know the way.",
            HomeHeroButtonText = "Explore the guide",
            AboutHeading = "Discover the beauty of Pakistan",
            AboutDescription = "Explore the landscapes, destinations and experiences that make Pakistan extraordinary.",
            FooterDescription = "Discover destinations, experiences and journeys across Pakistan.",
            DefaultMetaTitle = "Karnel Travel Guide | Explore Pakistan",
            DefaultMetaDescription = "Discover destinations, hotels, restaurants, resorts and travel experiences across Pakistan."
        };
    }
}
