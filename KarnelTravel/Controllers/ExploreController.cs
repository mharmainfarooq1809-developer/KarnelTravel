using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KarnelTravel.Controllers
{
    public class ExploreController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index() => View();
    }
}