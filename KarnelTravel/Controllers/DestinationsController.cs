using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class DestinationsController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Information/Dropdown/destination.cshtml");
        }
    }
}
