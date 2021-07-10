using System;
using System.Linq;
using System.Security.Claims;
using Eventuras.Domain;
using Eventuras.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace Eventuras.Services.Organizations
{
    internal static class OrganizationQueryableExtensions
    {
        public static IQueryable<Organization> AddFilter(this IQueryable<Organization> query, OrganizationFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (filter.InactiveOnly)
            {
                query = query.Where(o => !o.Active);
            }
            else if (!filter.IncludeInactive)
            {
                query = query.Where(o => o.Active);
            }

            return query;
        }

        public static IQueryable<Organization> WithOptions(this IQueryable<Organization> query,
            OrganizationRetrievalOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.LoadMembers)
            {
                query = query.Include(o => o.Members)
                    .ThenInclude(m => m.Roles);
            }

            if (options.LoadHostnames)
            {
                query = query.Include(o => o.Hostnames);
            }

            if (options.LoadSettings)
            {
                query = query.Include(o => o.Settings);
            }

            return query;
        }

        public static IQueryable<Organization> AddOrder(
            this IQueryable<Organization> query,
            OrganizationListOrder order,
            bool descending = false)
        {
            switch (order)
            {
                case OrganizationListOrder.Name:
                    query = descending
                        ? query.OrderByDescending(o => o.Name)
                        : query.OrderBy(o => o.Name);
                    break;
            }

            return query;
        }

        public static IQueryable<Organization> HasOrganizationMember(this IQueryable<Organization> query,
            ClaimsPrincipal user)
        {
            return query.Where(o => o.Members.Any(m => m.UserId == user.GetUserId()));
        }
    }
}
