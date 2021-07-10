using Eventuras.Domain;
using Eventuras.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eventuras.Services.Exceptions;

namespace Eventuras.Services.Organizations
{
    internal class CurrentOrganizationAccessorService : ICurrentOrganizationAccessorService
    {
        private const string OrgIdParamName = "orgId";

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentOrganizationAccessorService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Organization> GetCurrentOrganizationAsync(
            OrganizationRetrievalOptions options,
            CancellationToken cancellationToken)
        {
            // Retrieve current organization by orgId param first
            var orgIdParamValue = _httpContextAccessor.HttpContext.Request.Query[OrgIdParamName];
            if (!string.IsNullOrEmpty(orgIdParamValue) && int.TryParse(orgIdParamValue, out var organizationId))
            {
                return await _context.Organizations
                    .AsNoTracking()
                    .WithOptions(options ?? new OrganizationRetrievalOptions())
                    .Where(o => o.OrganizationId == organizationId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // Try hostname approach, if no orgId is present in the query
            var host = _httpContextAccessor.HttpContext.Request.Host;
            if (!host.HasValue)
            {
                return null;
            }

            return await _context.Organizations
                .AsNoTracking()
                .WithOptions(options ?? new OrganizationRetrievalOptions())
                .Where(o => o.Hostnames.Any(h => h.Active && h.Hostname == host.Value))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Organization> RequireCurrentOrganizationAsync(
            OrganizationRetrievalOptions options,
            CancellationToken cancellationToken)
        {
            return await GetCurrentOrganizationAsync(options, cancellationToken) ??
                   throw new OrgNotSpecifiedException(_httpContextAccessor.HttpContext.Request.Host.Value);
        }
    }
}
