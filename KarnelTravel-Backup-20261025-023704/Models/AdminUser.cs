namespace KarnelTravel.Models;

public class AdminUser
{
    public int Id { get; set; } // This is auto-increment by default!
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Admin";
}