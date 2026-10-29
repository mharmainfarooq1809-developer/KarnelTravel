using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class HotelsController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Information/Dropdown/Hotels.cshtml");
        }
    }
}
