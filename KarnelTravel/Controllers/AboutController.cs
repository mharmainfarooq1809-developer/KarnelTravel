using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KarnelTravel.Services;

namespace KarnelTravel.Controllers
{
    public class AboutController : Controller
    {
        private readonly IWebsiteContentService _websiteContentService;

        public AboutController(IWebsiteContentService websiteContentService)
        {
            _websiteContentService = websiteContentService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.WebsiteContent = await _websiteContentService.GetAsync();
            return View();
        }
    }
}