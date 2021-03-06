using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Eventuras.Domain;
using Eventuras.Infrastructure;
using Eventuras.Services;
using System.ComponentModel.DataAnnotations;
using Eventuras.Services.Events;
using Eventuras.Web.Services;

namespace Eventuras.Pages.Admin.Events
{
    public class MessagingModel : PageModel
    {
        private readonly IEventInfoRetrievalService _eventinfos;

        public MessagingModel(
            IEventInfoRetrievalService eventinfos)
        {
            _eventinfos = eventinfos;
        }

        public EventInfo EventInfo { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            EventInfo = await _eventinfos.GetEventInfoByIdAsync(id);
            return Page();
        }

    }
}

