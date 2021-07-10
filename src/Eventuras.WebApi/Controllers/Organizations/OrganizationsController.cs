using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Eventuras.Domain;
using Eventuras.Services;
using Eventuras.Services.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventuras.WebApi.Controllers.Organizations
{
    [ApiVersion("3")]
    [Authorize(Roles = Roles.SystemAdmin)]
    [Route("v{version:apiVersion}/organizations")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationRetrievalService _organizationRetrievalService;
        private readonly IOrganizationManagementService _organizationManagementService;

        public OrganizationsController(
            IOrganizationRetrievalService organizationRetrievalService,
            IOrganizationManagementService organizationManagementService)
        {
            _organizationRetrievalService = organizationRetrievalService ?? throw
                new ArgumentNullException(nameof(organizationRetrievalService));

            _organizationManagementService = organizationManagementService ?? throw
                new ArgumentNullException(nameof(organizationManagementService));
        }

        [HttpGet]
        public async Task<OrganizationDto[]> List()
        {
            var orgs = await _organizationRetrievalService
                .ListOrganizationsAsync(new OrganizationListRequest());

            return orgs.Select(org => new OrganizationDto(org))
                .ToArray();
        }

        [HttpGet("{organizationId}")]
        public async Task<OrganizationDto> Get(int organizationId)
        {
            var org = await _organizationRetrievalService
                .GetOrganizationByIdAsync(organizationId);

            return new OrganizationDto(org);
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> Create([FromBody] OrganizationFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var org = new Organization();
            dto.CopyTo(org);

            await _organizationManagementService.CreateNewOrganizationAsync(org);

            return Ok(new OrganizationDto(org));
        }

        [HttpPut("{organizationId}")]
        public async Task<ActionResult<OrganizationDto>> Update(int organizationId, [FromBody] OrganizationFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var org = await _organizationRetrievalService.GetOrganizationByIdAsync(organizationId);
            dto.CopyTo(org);

            await _organizationManagementService.UpdateOrganizationAsync(org);

            return Ok(new OrganizationDto(org));
        }

        [HttpDelete("{organizationId}")]
        public async Task Delete(int organizationId)
        {
            await _organizationManagementService.DeleteOrganizationAsync(organizationId);
        }
    }

    public class OrganizationDto
    {
        public int OrganizationId { get; }
        public string Name { get; }
        public string Description { get; }
        public string Url { get; }
        public string Phone { get; }
        public string Email { get; }
        public string LogoUrl { get; }
        public string LogoBase64 { get; }

        public OrganizationDto(Organization org)
        {
            OrganizationId = org.OrganizationId;
            Name = org.Name;
            Description = org.Description;
            Url = org.Url;
            Phone = org.Phone;
            Email = org.Email;
            LogoUrl = org.LogoUrl;
            LogoBase64 = org.LogoBase64;
        }
    }

    public class OrganizationFormDto
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public string LogoBase64 { get; set; }

        public void CopyTo(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            organization.Name = Name;
            organization.Description = Description;
            organization.Url = Url;
            organization.Phone = Phone;
            organization.Email = Email;
            organization.LogoUrl = LogoUrl;
            organization.LogoBase64 = LogoBase64;
        }
    }
}
