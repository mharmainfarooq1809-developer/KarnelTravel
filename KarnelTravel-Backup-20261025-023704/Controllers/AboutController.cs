using Microsoft.AspNetCore.Mvc;

namespace KarnelTravel.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index() => View();
    }
}