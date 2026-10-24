using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class LandingController : Controller
    {
        // The root route can point to this action in Program.cs.
        public IActionResult Index()
        {
            return View();
        }
    }
}
