using System;
using System.Threading.Tasks;
using Eventuras.TestAbstractions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eventuras.WebApi.Tests.Controllers.Organizations
{
    public class OrganizationControllerTest : IClassFixture<CustomWebApiApplicationFactory<Startup>>, IDisposable
    {
        private readonly CustomWebApiApplicationFactory<Startup> _factory;

        public OrganizationControllerTest(CustomWebApiApplicationFactory<Startup> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void Dispose()
        {
            using var scope = _factory.Services.NewTestScope();
            scope.Db.Organizations.Clean();
            scope.Db.SaveChanges();
        }

        [Fact]
        public async Task List_Should_Require_Auth()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v3/organizations");
            response.CheckUnauthorized();
        }

        [Fact]
        public async Task List_Should_Require_System_Admin_Role()
        {
            var client = _factory.CreateClient().Authenticated();
            var response = await client.GetAsync("/v3/organizations");
            response.CheckForbidden();
        }

        [Fact]
        public async Task List_Should_Return_Empty_List()
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.GetAsync("/v3/organizations");
            var token = await response.CheckOk().AsArrayAsync();
            token.CheckEmptyArray();
        }

        [Fact]
        public async Task List_Should_Return_All_Organizations()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org3 = await scope.CreateOrganizationAsync("Org 3");
            using var org2 = await scope.CreateOrganizationAsync("Org 2");
            using var org1 = await scope.CreateOrganizationAsync("Org 1");

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.GetAsync("/v3/organizations");
            var token = await response.CheckOk().AsArrayAsync();
            token.CheckArray((t, o) => t.CheckOrganization(o),
                org1.Entity, org2.Entity, org3.Entity);
        }

        [Fact]
        public async Task Get_Should_Require_Auth()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v3/organizations/1");
            response.CheckUnauthorized();
        }

        [Fact]
        public async Task Get_Should_Require_System_Admin_Role()
        {
            var client = _factory.CreateClient().Authenticated();
            var response = await client.GetAsync("/v3/organizations/1");
            response.CheckForbidden();
        }

        [Fact]
        public async Task Get_Should_Return_Not_Found_For_Non_Existing_Org()
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.GetAsync("/v3/organizations/10001");
            response.CheckNotFound();
        }

        [Fact]
        public async Task Get_Should_Return_Not_Found_For_Removed_Org()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync(inactive: true);

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.GetAsync($"/v3/organizations/{org.Entity.OrganizationId}");
            response.CheckNotFound();
        }

        [Fact]
        public async Task Get_Should_Return_Org_Info()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync();

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.GetAsync($"/v3/organizations/{org.Entity.OrganizationId}");
            var token = await response.CheckOk().AsTokenAsync();
            token.CheckOrganization(org.Entity);
        }

        [Fact]
        public async Task Create_Should_Require_Auth()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/v3/organizations");
            response.CheckUnauthorized();
        }

        [Fact]
        public async Task Create_Should_Require_System_Admin_Role()
        {
            var client = _factory.CreateClient().Authenticated();
            var response = await client.PostAsync("/v3/organizations");
            response.CheckForbidden();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task Create_Should_Validate_Input(string name)
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PostAsync("/v3/organizations", new {name});
            response.CheckBadRequest();
        }

        [Fact]
        public async Task Should_Create_New_Org_With_Min_Data()
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PostAsync("/v3/organizations", new {name = "test"});
            var token = await response.CheckOk().AsTokenAsync();

            using var scope = _factory.Services.NewTestScope();
            var org = await scope.Db.Organizations.AsNoTracking()
                .SingleAsync(o => o.Name == "test");
            token.CheckOrganization(org);

            scope.Db.Organizations.Remove(org);
            await scope.Db.SaveChangesAsync();
        }

        [Fact]
        public async Task Create_Should_Save_Max_Org_Data()
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PostAsync("/v3/organizations", new
            {
                name = "test org",
                description = "some description",
                url = "http://some.url",
                phone = "+11111111111",
                email = "test@email.com",
                logoUrl = "http://test.org/logo.png",
                logoBase64 = "bG9nbwo="
            });
            var token = await response.CheckOk().AsTokenAsync();

            using var scope = _factory.Services.NewTestScope();
            var org = await scope.Db.Organizations.AsNoTracking()
                .SingleAsync(o => o.Name == "test org");

            Assert.Equal("test org", org.Name);
            Assert.Equal("some description", org.Description);
            Assert.Equal("http://some.url", org.Url);
            Assert.Equal("+11111111111", org.Phone);
            Assert.Equal("test@email.com", org.Email);
            Assert.Equal("http://test.org/logo.png", org.LogoUrl);
            Assert.Equal("bG9nbwo=", org.LogoBase64);
            token.CheckOrganization(org);

            scope.Db.Organizations.Remove(org);
            await scope.Db.SaveChangesAsync();
        }

        [Fact]
        public async Task Update_Should_Require_Auth()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsync("/v3/organizations/10001");
            response.CheckUnauthorized();
        }

        [Fact]
        public async Task Update_Should_Require_System_Admin_Role()
        {
            var client = _factory.CreateClient().Authenticated();
            var response = await client.PutAsync("/v3/organizations/10001");
            response.CheckForbidden();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task Update_Should_Validate_Input(string name)
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync();

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PutAsync($"/v3/organizations/{org.Entity.OrganizationId}", new {name});
            response.CheckBadRequest();
        }

        [Fact]
        public async Task Should_Update_Org_With_Min_Data()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync();

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PutAsync($"/v3/organizations/{org.Entity.OrganizationId}",
                new {name = "updated org name"});
            var token = await response.CheckOk().AsTokenAsync();

            var updatedOrg = await scope.Db.Organizations
                .AsNoTracking()
                .SingleAsync(o => o.OrganizationId == org.Entity.OrganizationId);
            Assert.Equal("updated org name", updatedOrg.Name);

            token.CheckOrganization(updatedOrg);
        }

        [Fact]
        public async Task Should_Update_Org_With_Max_Data()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync();

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.PutAsync($"/v3/organizations/{org.Entity.OrganizationId}",
                new
                {
                    name = "updated org name",
                    description = "some description",
                    url = "http://some.url",
                    phone = "+11111111111",
                    email = "test@email.com",
                    logoUrl = "http://test.org/logo.png",
                    logoBase64 = "bG9nbwo="
                });
            var token = await response.CheckOk().AsTokenAsync();

            var updatedOrg = await scope.Db.Organizations
                .AsNoTracking()
                .SingleAsync(o => o.OrganizationId == org.Entity.OrganizationId);
            Assert.Equal("updated org name", updatedOrg.Name);
            Assert.Equal("some description", updatedOrg.Description);
            Assert.Equal("http://some.url", updatedOrg.Url);
            Assert.Equal("+11111111111", updatedOrg.Phone);
            Assert.Equal("test@email.com", updatedOrg.Email);
            Assert.Equal("http://test.org/logo.png", updatedOrg.LogoUrl);
            Assert.Equal("bG9nbwo=", updatedOrg.LogoBase64);

            token.CheckOrganization(updatedOrg);
        }

        [Fact]
        public async Task Should_Require_Auth_To_Delete_Org()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync("/v3/organizations/10001");
            response.CheckUnauthorized();
        }

        [Fact]
        public async Task Should_Require_System_Admin_Role_To_Delete_Org()
        {
            var client = _factory.CreateClient().Authenticated();
            var response = await client.DeleteAsync("/v3/organizations/10001");
            response.CheckForbidden();
        }

        [Fact]
        public async Task Should_Return_Not_Found_When_Deleting_Unexisting_Org()
        {
            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.DeleteAsync("/v3/organizations/10001");
            response.CheckNotFound();
        }

        [Fact]
        public async Task Should_Mark_Org_As_Inactive_After_Deletion()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync();

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.DeleteAsync($"/v3/organizations/{org.Entity.OrganizationId}");
            response.CheckOk();

            var updatedOrg = await scope.Db.Organizations
                .AsNoTracking()
                .SingleAsync(o => o.OrganizationId == org.Entity.OrganizationId);

            Assert.False(updatedOrg.Active);
        }

        [Fact]
        public async Task Should_Return_Ok_If_Org_Is_Already_Deleted()
        {
            using var scope = _factory.Services.NewTestScope();
            using var org = await scope.CreateOrganizationAsync(inactive: true);

            var client = _factory.CreateClient().AuthenticatedAsSystemAdmin();
            var response = await client.DeleteAsync($"/v3/organizations/{org.Entity.OrganizationId}");
            response.CheckOk();

            var updatedOrg = await scope.Db.Organizations
                .AsNoTracking()
                .SingleAsync(o => o.OrganizationId == org.Entity.OrganizationId);

            Assert.False(updatedOrg.Active);
        }
    }
}
