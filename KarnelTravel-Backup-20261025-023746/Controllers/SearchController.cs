using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Advance()
        {
            return View();
        }

        // Provide legacy route name used in layout (Advanced)
        public IActionResult Advanced()
        {
            return View("Advance");
        }
    }
}