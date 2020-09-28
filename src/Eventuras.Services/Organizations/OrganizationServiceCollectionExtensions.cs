using Microsoft.Extensions.DependencyInjection;

namespace Eventuras.Services.Organizations
{
    internal static class OrganizationServiceCollectionExtensions
    {
        public static IServiceCollection AddOrganizationServices(this IServiceCollection services)
        {
            services.AddTransient<ICurrentOrganizationAccessorService, CurrentOrganizationAccessorService>();
            services.AddTransient<IOrganizationRetrievalService, OrganizationRetrievalService>();
            services.AddTransient<IOrganizationManagementService, OrganizationManagementService>();
            return services;
        }
    }
}
