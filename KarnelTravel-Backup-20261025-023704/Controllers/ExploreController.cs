using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class ExploreController : Controller
    {
        public IActionResult Index() => View();
    }
}