using System.Threading.Tasks;
using Eventuras.Domain;

namespace Eventuras.Services.Organizations
{
    public interface IOrganizationManagementService
    {
        /// <summary>
        /// Create new org is available to <see cref="Roles.SuperAdmin"/> role only.
        /// </summary>
        /// <param name="organization">Not <c>null</c></param>
        /// <exception cref="Exceptions.NotAccessibleException">Can't create this organization.</exception>
        Task CreateNewOrganizationAsync(Organization organization);

        /// <summary>
        /// Update org is available to <see cref="Roles.SuperAdmin"/> role only.
        /// </summary>
        /// <param name="organization">Not <c>null</c></param>
        /// <exception cref="Exceptions.NotAccessibleException">Can't update this organization.</exception>
        Task UpdateOrganizationAsync(Organization organization);

        /// <param name="id">Organization id.</param>
        /// <param name="hostnames">Not null. If empty, all org hostnames will be removed.</param>
        /// <exception cref="Exceptions.NotFoundException">Organization not found or was deleted.</exception>
        /// <exception cref="Exceptions.DuplicateException">Duplicate hostname.</exception>
        /// <exception cref="Exceptions.NotAccessibleException">Can't update this organization.</exception>
        Task UpdateOrganizationHostnames(int id, string[] hostnames);

        /// <exception cref="Exceptions.NotFoundException">Organization not found.</exception>
        /// <exception cref="Exceptions.NotAccessibleException">Can't delete this organization.</exception>
        Task DeleteOrganizationAsync(int id);
    }
}
