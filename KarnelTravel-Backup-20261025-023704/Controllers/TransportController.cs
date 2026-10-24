using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class TransportController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Information/Dropdown/Transport.cshtml");
        }
    }
}
