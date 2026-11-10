using KarnelTravel.Models;

namespace KarnelTravel.Services;

public interface ITripInquiryEmailService
{
    Task SendOfferAsync(TripInquiry inquiry, CancellationToken cancellationToken = default);
}
