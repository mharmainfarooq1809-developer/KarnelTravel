using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public sealed class AIQuickPrompt
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Label { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Prompt { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}
