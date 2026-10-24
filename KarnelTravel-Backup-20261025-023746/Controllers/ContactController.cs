using KarnelTravel.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace KarnelTravel.Controllers
{
    public class ContactController : Controller
    {
        // Simple in-memory store for demo purposes.
        private static readonly List<ContactInquiry> _inquiries = new List<ContactInquiry>();
        private static int _nextId = 1;

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactInquiry());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ContactInquiry model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = _nextId++;
            _inquiries.Add(model);

            TempData["ContactSuccess"] = "Thank you — your inquiry has been received. We will contact you soon.";
            return RedirectToAction(nameof(Index));
        }

        // For basic admin/demo viewing of inquiries (not linked in nav)
        [HttpGet]
        public IActionResult Submissions()
        {
            return View(_inquiries.OrderByDescending(i => i.SubmittedAt).ToList());
        }
    }
}
