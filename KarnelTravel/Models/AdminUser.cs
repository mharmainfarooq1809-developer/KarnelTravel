namespace KarnelTravelGuide.Models;

/// <summary>
/// Admin user account. In-memory for the prototype; replace with an
/// EF Core entity mapped to SQL Server in production.
/// </summary>
public class AdminUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Admin";
}