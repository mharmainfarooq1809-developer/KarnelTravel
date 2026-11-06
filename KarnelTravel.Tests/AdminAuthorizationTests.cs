using KarnelTravel.Controllers;
using KarnelTravel.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace KarnelTravel.Tests;

public sealed class AdminAuthorizationTests
{
    [Fact]
    public void Admin_ai_controller_requires_admin_only_policy()
    {
        var attribute = typeof(AdminAIController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("AdminOnly", attribute!.Policy);
    }

    [Fact]
    public void Admin_mutations_require_antiforgery()
    {
        var methods = typeof(AdminAIController).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(x => x.Name is "Configuration" or "Capability" or "Recommendations" or "SaveKnowledge" or "DeleteKnowledge" or "ToggleKnowledge" or "SavePrompt" or "DeletePrompt" or "TogglePrompt").Where(x => x.GetCustomAttribute<HttpPostAttribute>() is not null);
        Assert.NotEmpty(methods);
        Assert.All(methods, method => Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>()));
    }
}
