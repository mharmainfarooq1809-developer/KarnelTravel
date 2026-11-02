using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class ExperiencesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}