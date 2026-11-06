using KarnelTravel.Models;
using KarnelTravel.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/AI")]
public sealed class AdminAIController : Controller
{
    private readonly IAIAdministrationService _admin;
    public AdminAIController(IAIAdministrationService admin) => _admin = admin;

    [HttpGet("")]
    [HttpGet("Overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken) => View("Overview", await _admin.GetDashboardAsync(cancellationToken));

    [HttpGet("Configuration")]
    public async Task<IActionResult> Configuration(CancellationToken cancellationToken) => View(await _admin.GetConfigurationAsync(cancellationToken));

    [HttpPost("Configuration")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Configuration(AIConfiguration model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        await _admin.SaveConfigurationAsync(model, User.Identity?.Name, cancellationToken);
        TempData["StatusMessage"] = "Karnel AI configuration saved.";
        return RedirectToAction(nameof(Configuration));
    }

    [HttpGet("Capabilities")]
    public async Task<IActionResult> Capabilities(CancellationToken cancellationToken) => View(await _admin.GetCapabilitiesAsync(cancellationToken));

    [HttpPost("Capabilities/{key}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Capability(string key, bool enabled, CancellationToken cancellationToken)
    {
        await _admin.SetCapabilityAsync(key, enabled, User.Identity?.Name, cancellationToken);
        return RedirectToAction(nameof(Capabilities));
    }

    [HttpGet("Recommendations")]
    public async Task<IActionResult> Recommendations(CancellationToken cancellationToken) => View(await _admin.GetRecommendationSettingsAsync(cancellationToken));

    [HttpPost("Recommendations")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recommendations(AIRecommendationSettings model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try { await _admin.SaveRecommendationSettingsAsync(model, User.Identity?.Name, cancellationToken); TempData["StatusMessage"] = "Recommendation settings saved."; return RedirectToAction(nameof(Recommendations)); }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
    }

    [HttpGet("Knowledge")]
    public async Task<IActionResult> Knowledge(string? search, CancellationToken cancellationToken) => View(await _admin.GetKnowledgeAsync(search, cancellationToken));

    [HttpGet("Knowledge/Create")]
    public IActionResult CreateKnowledge() => View("KnowledgeEdit", new AIKnowledgeEntry());

    [HttpGet("Knowledge/Edit/{id:int}")]
    public async Task<IActionResult> EditKnowledge(int id, CancellationToken cancellationToken) => (await _admin.GetKnowledgeEntryAsync(id, cancellationToken)) is { } entry ? View("KnowledgeEdit", entry) : NotFound();

    [HttpPost("Knowledge/Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveKnowledge(AIKnowledgeEntry model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("KnowledgeEdit", model);
        try { await _admin.SaveKnowledgeEntryAsync(model, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Knowledge)); }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View("KnowledgeEdit", model); }
    }

    [HttpPost("Knowledge/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteKnowledge(int id, CancellationToken cancellationToken) { await _admin.DeleteKnowledgeEntryAsync(id, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Knowledge)); }

    [HttpPost("Knowledge/Toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleKnowledge(int id, bool enabled, CancellationToken cancellationToken) { await _admin.SetKnowledgeEntryEnabledAsync(id, enabled, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Knowledge)); }

    [HttpGet("Prompts")]
    public async Task<IActionResult> Prompts(CancellationToken cancellationToken) => View(await _admin.GetPromptsAsync(cancellationToken));

    [HttpGet("Prompts/Create")]
    public IActionResult CreatePrompt() => View("PromptEdit", new AIQuickPrompt());

    [HttpGet("Prompts/Edit/{id:int}")]
    public async Task<IActionResult> EditPrompt(int id, CancellationToken cancellationToken) => (await _admin.GetPromptAsync(id, cancellationToken)) is { } prompt ? View("PromptEdit", prompt) : NotFound();

    [HttpPost("Prompts/Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePrompt(AIQuickPrompt model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("PromptEdit", model);
        try { await _admin.SavePromptAsync(model, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Prompts)); }
        catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View("PromptEdit", model); }
    }

    [HttpPost("Prompts/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePrompt(int id, CancellationToken cancellationToken) { await _admin.DeletePromptAsync(id, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Prompts)); }

    [HttpPost("Prompts/Toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePrompt(int id, bool enabled, CancellationToken cancellationToken) { await _admin.SetPromptEnabledAsync(id, enabled, User.Identity?.Name, cancellationToken); return RedirectToAction(nameof(Prompts)); }

    [HttpGet("Conversations")]
    public async Task<IActionResult> Conversations(CancellationToken cancellationToken) => View(await _admin.GetConversationsAsync(cancellationToken));

    [HttpGet("Feedback")]
    public async Task<IActionResult> Feedback(bool? helpful, CancellationToken cancellationToken) => View(await _admin.GetFeedbackAsync(helpful, cancellationToken));

    [HttpGet("AuditLogs")]
    public async Task<IActionResult> AuditLogs(CancellationToken cancellationToken) => View(await _admin.GetAuditLogsAsync(cancellationToken));

    [HttpGet("Health")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        var configuration = await _admin.GetConfigurationAsync(cancellationToken);
        return View(new AIHealthViewModel { EngineAvailable = true, DatabaseAvailable = true, ConfigurationAvailable = configuration.Id > 0, LastChecked = DateTime.UtcNow });
    }
}

public sealed class AIHealthViewModel
{
    public bool EngineAvailable { get; init; }
    public bool DatabaseAvailable { get; init; }
    public bool ConfigurationAvailable { get; init; }
    public DateTime LastChecked { get; init; }
}
