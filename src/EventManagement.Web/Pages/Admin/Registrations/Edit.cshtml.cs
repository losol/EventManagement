using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using losol.EventManagement.Domain;
using losol.EventManagement.Infrastructure;
using losol.EventManagement.Services;

namespace losol.EventManagement.Pages.Admin.Registrations
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentMethodService _paymentMethods;

        public EditModel(ApplicationDbContext context, IPaymentMethodService paymentMethods)
        {
            _context = context;
            _paymentMethods = paymentMethods;
        }

        [BindProperty]
        public Registration Registration { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Registration = await _context.Registrations
                .Include(r => r.EventInfo)
                .Include(r => r.User)
                .SingleOrDefaultAsync(m => m.RegistrationId == id);

            if (Registration == null)
            {
                return NotFound();
            }
           ViewData["EventInfoId"] = new SelectList(_context.EventInfos, "EventInfoId", "Code");
           ViewData["PaymentMethod"] = new SelectList(_paymentMethods.GetActivePaymentMethods(), "Provider", "Name");
           ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Registration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistrationExists(Registration.RegistrationId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(e => e.RegistrationId == id);
        }
    }
}
