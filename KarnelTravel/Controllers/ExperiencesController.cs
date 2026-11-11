using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers;

[AllowAnonymous]
public sealed class ExperiencesController : Controller
{
    public IActionResult Index() => View();
}
