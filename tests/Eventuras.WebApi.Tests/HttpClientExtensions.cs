using Eventuras.TestAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Eventuras.Domain;
using Eventuras.Services;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Eventuras.WebApi.Tests
{
    internal static class HttpClientExtensions
    {
        public static HttpClient AuthenticatedAsSystemAdmin(this HttpClient httpClient)
        {
            return httpClient.Authenticated(role: Roles.SystemAdmin);
        }

        public static HttpClient AuthenticatedAsSuperAdmin(this HttpClient httpClient)
        {
            return httpClient.Authenticated(role: Roles.SuperAdmin);
        }

        public static HttpClient AuthenticatedAs(this HttpClient httpClient, ApplicationUser user, params string[] roles)
        {
            return httpClient.Authenticated(
                user.Id,
                user.Name,
                null, // FIXME: we don't have surname in AspNetCore.Identity?
                user.Email,
                roles: roles);
        }

        public static HttpClient Authenticated(
            this HttpClient httpClient,
            string id = TestingConstants.Placeholder,
            string firstName = TestingConstants.Placeholder,
            string lastName = TestingConstants.Placeholder,
            string email = TestingConstants.Placeholder,
            string role = null,
            string[] roles = null,
            string[] scopes = null)
        {
            var claims = BuildClaims(id, firstName, lastName, email, role, roles, scopes);
            var token = FakeJwtManager.GenerateJwtToken(claims);
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return httpClient;
        }

        private static Claim[] BuildClaims(
            string id,
            string firstName,
            string lastName,
            string email,
            string role,
            string[] roles,
            string[] scopes)
        {
            if (TestingConstants.Placeholder.Equals(id))
            {
                id = Guid.NewGuid().ToString();
            }
            if (TestingConstants.Placeholder.Equals(firstName))
            {
                firstName = "Test";
            }
            if (TestingConstants.Placeholder.Equals(lastName))
            {
                lastName = $"Person {Guid.NewGuid()}";
            }
            if (TestingConstants.Placeholder.Equals(email))
            {
                email = $"test-person+{Guid.NewGuid()}@email.com";
            }
            scopes ??= TestingConstants.DefaultScopes;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim("scope", string.Join(" ", scopes))
            };
            if (!string.IsNullOrEmpty(firstName))
            {
                claims.Add(new Claim(ClaimTypes.Name, firstName));
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                claims.Add(new Claim(ClaimTypes.Surname, lastName));
            }
            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Surname, email));
            }
            if (roles == null && role != null)
            {
                roles = new[] { role };
            }
            if (roles != null && roles.Any())
            {
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }
            return claims.ToArray();
        }

        public static async Task<HttpResponseMessage> PostAsync(
            this HttpClient httpClient,
            string requestUri,
            object data)
        {
            return await httpClient.PostAsync(requestUri,
                new StringContent(JsonConvert.SerializeObject(data),
                    Encoding.UTF8, "application/json"));
        }

        public static async Task<HttpResponseMessage> PutAsync(
            this HttpClient httpClient,
            string requestUri,
            object data)
        {
            return await httpClient.PutAsync(requestUri,
                new StringContent(JsonConvert.SerializeObject(data),
                    Encoding.UTF8, "application/json"));
        }
    }
}
