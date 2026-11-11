using KarnelTravel.Services.AI;
using KarnelTravel.Data;
using KarnelTravel.Models;
using KarnelTravel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KarnelTravel.Controllers;

[Route("KarnelAI")]
public sealed class KarnelAiController : Controller
{
    private readonly IAIEngine _ai;
    private readonly ITravelSearchService _search;
    private readonly IAIActionService _actions;
    private readonly ApplicationDbContext _db;
    private readonly IAIControlledActionService _controlledActions;
    private readonly IAIAdministrationService _administration;

    public KarnelAiController(IAIEngine ai, ITravelSearchService search, IAIActionService actions, ApplicationDbContext db, IAIControlledActionService controlledActions, IAIAdministrationService administration) { _ai = ai; _search = search; _actions = actions; _db = db; _controlledActions = controlledActions; _administration = administration; }

    [AllowAnonymous]
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(new KarnelAiPageViewModel
        {
            EngineAvailable = true,
            Listings = (await _search.SearchAsync(new AISearchRequest(null, null, null, null), cancellationToken)).Items.ToList(),
            QuickPrompts = (await _administration.GetPromptsAsync(cancellationToken)).Where(x => x.IsEnabled).OrderBy(x => x.DisplayOrder).Select(x => x.Prompt).ToList()
        });
    }

    [HttpPost("Compare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compare([FromBody] KarnelAiCompareRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(new { success = false, error = "Select between two and four listings." });
        return Ok(await _actions.CompareAsync(request.ListingIds, cancellationToken));
    }

    [HttpPost("Feedback")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Feedback([FromBody] KarnelAiFeedbackRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(new { success = false, error = "Feedback is invalid." });
        _db.AIFeedback.Add(new AIFeedback { ConversationKey = request.ConversationKey.Trim(), IsHelpful = request.IsHelpful, Comment = request.Comment?.Trim() });
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { success = true });
    }

    [HttpPost("ContactInquiry/Prepare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrepareContactInquiry([FromBody] AIInquiryRequest request, CancellationToken cancellationToken) => Ok(await _controlledActions.PrepareContactInquiryAsync(request, cancellationToken));

    [HttpPost("ContactInquiry/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitContactInquiry([FromBody] AIInquiryRequest request, CancellationToken cancellationToken) => Ok(await _controlledActions.SubmitContactInquiryAsync(request, User.Identity?.Name, cancellationToken));

    [HttpPost("BookingInquiry/Prepare")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrepareBookingInquiry([FromBody] AIInquiryRequest request, CancellationToken cancellationToken) => Ok(await _controlledActions.PrepareBookingInquiryAsync(request, cancellationToken));

    [HttpPost("BookingInquiry/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBookingInquiry([FromBody] AIInquiryRequest request, CancellationToken cancellationToken) => Ok(await _controlledActions.SubmitBookingInquiryAsync(request, User.Identity?.Name, cancellationToken));

    [HttpPost("BookingStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookingStatus([FromBody] AIBookingStatusRequest request, CancellationToken cancellationToken) => Ok(await _controlledActions.GetBookingStatusAsync(request, User.Identity?.Name, cancellationToken));

    [HttpPost("Chat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chat([FromBody] KarnelAiChatRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new KarnelAiChatResponse { Error = "Please enter a travel question." });

        var inputMode = string.Equals(request.InputMode, "voice", StringComparison.OrdinalIgnoreCase) ? "voice" : "text";
        var requestId = string.IsNullOrWhiteSpace(request.RequestId) ? Guid.NewGuid().ToString("N") : request.RequestId;
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var anonymousOwnerKey = GetAnonymousOwnerKey();
        var result = await _ai.ProcessAsync(new AIRequest(request.Message.Trim(), request.ConversationKey ?? Guid.NewGuid().ToString("N"), ownerEmail, User.IsInRole("Admin"), anonymousOwnerKey), cancellationToken);
        return Ok(new KarnelAiChatResponse { Success = true, Reply = result.Text, Sources = result.Listings, Intent = result.Intent.ToString(), ConversationKey = result.ConversationKey, InputMode = inputMode, RequestId = requestId });
    }

    [HttpPost("Plan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Plan([FromBody] KarnelAiPlannerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new KarnelAiPlannerResponse { Error = "Enter a destination and a trip duration from 1 to 60 days." });

        var ownerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
        var result = await _ai.ProcessAsync(new AIRequest($"Plan a {request.DurationDays} day trip to {request.Destination}. Budget {request.Budget}", request.ConversationKey ?? Guid.NewGuid().ToString("N"), ownerEmail, User.IsInRole("Admin"), GetAnonymousOwnerKey()), cancellationToken);
        return Ok(new KarnelAiPlannerResponse { Success = true, Itinerary = result.Text, Sources = result.Listings, ConversationKey = result.ConversationKey });
    }

    private string GetAnonymousOwnerKey()
    {
        const string cookieName = "KarnelAI.AnonymousOwner";
        if (Request.Cookies.TryGetValue(cookieName, out var existing) && Guid.TryParse(existing, out _))
            return existing;

        var value = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(cookieName, value, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = Request.IsHttps,
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(30)
        });
        return value;
    }

}
