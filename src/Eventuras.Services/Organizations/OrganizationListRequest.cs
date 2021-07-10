namespace Eventuras.Services.Organizations
{
    public class OrganizationListRequest
    {
        public OrganizationListOrder OrderBy { get; set; } = OrganizationListOrder.Name;

        public bool Descending { get; set; }
    }
}
