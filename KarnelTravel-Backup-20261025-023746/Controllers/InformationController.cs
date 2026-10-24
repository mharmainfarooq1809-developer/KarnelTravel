using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class InformationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/destinations")]
        public IActionResult Destination()
        {
            return View("Dropdown/destination");
        }

        [HttpGet("/transport")]
        public IActionResult Transport()
        {
            return View("Dropdown/transport");
        }

        [HttpGet("/hotels")]
        public IActionResult Hotels()
        {
            return View("Dropdown/Hotels");
        }
    }
}