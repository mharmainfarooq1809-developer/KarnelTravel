
using KarnelTravel.Data;
using KarnelTravel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarnelTravel.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ContactController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Display contact form
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactInquiry());
        }

        // Save contact inquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactInquiry model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = 0;
            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();
            model.Phone = model.Phone?.Trim();
            model.Category = model.Category.Trim();
            model.Message = model.Message.Trim();

            model.SubmittedAt = DateTime.UtcNow;
            model.Status = "New";
            model.AdminNotes = null;

            _db.ContactInquiries.Add(model);

            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] =
                "Thank you — your inquiry has been received. We will contact you soon.";

            return RedirectToAction(nameof(Index));
        }
    }
}
