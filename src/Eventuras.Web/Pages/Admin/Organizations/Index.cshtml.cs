using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventuras.Domain;
using Eventuras.Services.Organizations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eventuras.Web.Pages.Admin.Organizations
{
    public class IndexModel : PageModel
    {
        private readonly IOrganizationRetrievalService _organizationRetrievalService;

        public IndexModel(IOrganizationRetrievalService organizationRetrievalService)
        {
            _organizationRetrievalService = organizationRetrievalService ??
                                            throw new ArgumentNullException(nameof(organizationRetrievalService));
        }

        public IList<Organization> Organizations { get; set; }

        public async Task OnGetAsync()
        {
            Organizations = await _organizationRetrievalService.ListOrganizationsAsync(new OrganizationListRequest());
        }
    }
}
