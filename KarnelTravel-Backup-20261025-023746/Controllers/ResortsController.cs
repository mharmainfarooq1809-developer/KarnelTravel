using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class ResortsController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Information/Dropdown/Resorts.cshtml");
        }
    }
}
