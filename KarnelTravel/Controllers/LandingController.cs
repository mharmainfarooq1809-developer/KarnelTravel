using Microsoft.AspNetCore.Mvc;
using KarnelTravel.Services;

namespace KarnelTravel.Controllers
{
    public class LandingController : Controller
    {
        private readonly IWebsiteContentService _websiteContentService;

        public LandingController(IWebsiteContentService websiteContentService)
        {
            _websiteContentService = websiteContentService;
        }

        // The root route can point to this action in Program.cs.
        public async Task<IActionResult> Index()
        {
            ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
            return View();
        }
    }
}
