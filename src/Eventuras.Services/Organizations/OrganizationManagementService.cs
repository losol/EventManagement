using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eventuras.Domain;
using Eventuras.Infrastructure;
using Eventuras.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventuras.Services.Organizations
{
    internal class OrganizationManagementService : IOrganizationManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrganizationManagementService> _logger;
        private readonly IOrganizationAccessControlService _organizationAccessControlService;

        public OrganizationManagementService(
            ApplicationDbContext context,
            ILogger<OrganizationManagementService> logger,
            IOrganizationAccessControlService organizationAccessControlService)
        {
            _context = context ?? throw
                new ArgumentNullException(nameof(context));
            _logger = logger ?? throw
                new ArgumentNullException(nameof(logger));
            _organizationAccessControlService = organizationAccessControlService ?? throw
                new ArgumentNullException(nameof(organizationAccessControlService));
        }

        public async Task CreateNewOrganizationAsync(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            await _organizationAccessControlService
                .CheckOrganizationUpdateAccessAsync(organization.OrganizationId);

            try
            {
                await _context.CreateAsync(organization);
            }
            catch (DbUpdateException e) when (e.IsUniqueKeyViolation())
            {
                _logger.LogWarning(e, e.Message);
                _context.Remove(organization);
                throw new DuplicateException();
            }
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            await _organizationAccessControlService
                .CheckOrganizationUpdateAccessAsync(organization.OrganizationId);

            try
            {
                _context.Organizations.Update(organization);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.IsUniqueKeyViolation())
            {
                _logger.LogWarning(e, e.Message);
                _context.Entry(organization).State = EntityState.Detached;
                throw new DuplicateException();
            }
        }

        public async Task UpdateOrganizationHostnames(int id, string[] hostnames)
        {
            if (hostnames == null)
            {
                throw new ArgumentNullException(nameof(hostnames));
            }

            var organization = await _context.Organizations
                                   .Include(o => o.Hostnames)
                                   .SingleOrDefaultAsync(o => o.OrganizationId == id)
                               ?? throw new NotFoundException($"Organization {id} not found");

            if (!organization.Active)
            {
                throw new NotFoundException($"Organization {id} is deleted");
            }

            var normalizedHostnames = hostnames
                .Select(NormalizeHostname)
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Distinct()
                .ToArray();

            foreach (var hostname in normalizedHostnames)
            {
                if (await _context.OrganizationHostnames
                    .AnyAsync(h => h.OrganizationId != id &&
                                   h.Active &&
                                   h.Hostname == hostname))
                {
                    throw new DuplicateException($"Duplicate org hostname: {hostname}");
                }
            }

            var oldHostnames = organization.Hostnames
                .ToDictionary(h => h.Hostname);

            foreach (var hostname in normalizedHostnames)
            {
                if (oldHostnames.ContainsKey(hostname))
                {
                    oldHostnames[hostname].Active = true;
                    oldHostnames.Remove(hostname); // still actual, don't trash it
                }
                else
                {
                    await _context.CreateAsync(new OrganizationHostname
                    {
                        OrganizationId = id,
                        Hostname = hostname,
                        Active = true
                    });
                }
            }

            _context.OrganizationHostnames.RemoveRange(oldHostnames.Values);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrganizationAsync(int id)
        {
            await _organizationAccessControlService.CheckOrganizationUpdateAccessAsync(id);

            var organization = await _context.Organizations
                                   .Include(o => o.Hostnames)
                                   .SingleOrDefaultAsync(o => o.OrganizationId == id)
                               ?? throw new NotFoundException($"Organization {id} not found");

            if (!organization.Active)
            {
                return; // already deleted
            }

            organization.Active = false;
            organization.Hostnames.ForEach(h => h.Active = false);

            await _context.SaveChangesAsync();
        }

        private static string NormalizeHostname(string h)
        {
            if (string.IsNullOrWhiteSpace(h))
            {
                return null;
            }

            h = h.Trim().ToLower();

            if (!h.StartsWith("http") || !h.Contains("://"))
            {
                return h;
            }

            var m = HostPattern.Match(h);
            return m.Success ? m.Groups[2].Value : h;
        }

        private static readonly Regex HostPattern = new Regex("^http(s?):\\/\\/(.*?)\\/");
    }
}
