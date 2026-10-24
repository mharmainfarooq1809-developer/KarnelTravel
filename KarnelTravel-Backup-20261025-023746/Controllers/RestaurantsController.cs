using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class RestaurantsController : Controller
    {
        public IActionResult Index()
        {
            // Note: the view file has a typo 'Resturants.cshtml' in the project.
            return View("~/Views/Information/Dropdown/Resturants.cshtml");
        }
    }
}
