using System;
using System.Linq;
using Eventuras.Domain;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Eventuras.WebApi.Tests
{
    public static class JTokenExtensions
    {
        public static void CheckUser(this JToken token, ApplicationUser user)
        {
            Assert.Equal(user.Id, token.Value<string>("id"));
            Assert.Equal(user.Name, token.Value<string>("name"));
            Assert.Equal(user.Email, token.Value<string>("email"));
            Assert.Equal(user.PhoneNumber, token.Value<string>("phoneNumber"));
        }

        public static void CheckRegistration(this JToken token, Registration registration,
            bool checkUserInfo = false,
            bool checkEventInfo = false)
        {
            Assert.Equal(registration.RegistrationId, token.Value<int>("registrationId"));
            Assert.Equal(registration.EventInfoId, token.Value<int>("eventId"));
            Assert.Equal(registration.UserId, token.Value<string>("userId"));
            Assert.Equal(registration.Status.ToString(), token.Value<string>("status"));
            Assert.Equal(registration.Type.ToString(), token.Value<string>("type"));
            Assert.Equal(registration.Notes, token.Value<string>("notes"));

            if (checkUserInfo)
            {
                token.Value<JToken>("user").CheckUser(registration.User);
            }

            if (checkEventInfo)
            {
                token.Value<JToken>("event").CheckEvent(registration.EventInfo);
            }
        }

        public static void CheckEvent(this JToken token, EventInfo eventInfo)
        {
            Assert.Equal(eventInfo.EventInfoId, token.Value<int>("id"));
            Assert.Equal(eventInfo.Type.ToString(), token.Value<string>("type"));
            Assert.Equal(eventInfo.Title, token.Value<string>("title"));
            Assert.Equal(eventInfo.Slug, token.Value<string>("slug"));
            Assert.Equal(eventInfo.Category, token.Value<string>("category"));
            Assert.Equal(eventInfo.Description, token.Value<string>("description"));
            Assert.Equal(eventInfo.Program, token.Value<string>("program"));
            Assert.Equal(eventInfo.PracticalInformation, token.Value<string>("practicalInformation"));
            Assert.Equal(eventInfo.OnDemand, token.Value<bool>("onDemand"));
            Assert.Equal(eventInfo.Featured, token.Value<bool>("featured"));
            Assert.Equal(eventInfo.DateStart, token.Value<DateTime?>("dateStart"));
            Assert.Equal(eventInfo.DateEnd, token.Value<DateTime?>("dateEnd"));
            Assert.Equal(eventInfo.LastRegistrationDate, token.Value<DateTime?>("lastRegistrationDate"));
            Assert.Equal(eventInfo.Location, token.Value<string>("location"));
            Assert.Equal(eventInfo.City, token.Value<string>("city"));
        }

        public static void CheckOrder(this JToken token, Order order)
        {
            Assert.Equal(order.OrderId, token.Value<int>("orderId"));
            token.Value<JArray>("items")
                .CheckArray((t, item) => t.CheckOrderItem(item),
                    order.OrderLines.ToArray());
        }

        public static void CheckOrderItem(this JToken token, OrderLine item)
        {
            if (item.ProductId.HasValue)
            {
                token.Value<JToken>("product").CheckProduct(item.Product);
            }

            if (item.ProductVariantId.HasValue)
            {
                token.Value<JToken>("productVariant").CheckProductVariant(item.ProductVariant);
            }

            Assert.Equal(item.Quantity, token.Value<int>("quantity"));
        }

        public static void CheckProduct(this JToken token, Product product, params ProductVariant[] variants)
        {
            Assert.Equal(product.ProductId, token.Value<int>("productId"));
            Assert.Equal(product.Name, token.Value<string>("name"));
            Assert.Equal(product.Description, token.Value<string>("description"));
            Assert.Equal(product.MoreInformation, token.Value<string>("more"));
            Assert.Equal(product.Price, token.Value<decimal>("price"));
            Assert.Equal(product.VatPercent, token.Value<int>("vatPercent"));

            if (variants.Any())
            {
                token.Value<JArray>("variants")
                    .CheckArray((t, v) => t
                        .CheckProductVariant(v), variants);
            }
        }

        public static void CheckProductVariant(this JToken token, ProductVariant productVariant)
        {
            Assert.Equal(productVariant.ProductVariantId, token.Value<int>("productVariantId"));
            Assert.Equal(productVariant.Name, token.Value<string>("name"));
            Assert.Equal(productVariant.Description, token.Value<string>("description"));
            Assert.Equal(productVariant.Price, token.Value<decimal>("price"));
            Assert.Equal(productVariant.VatPercent, token.Value<int>("vatPercent"));
        }

        public static void CheckOrganization(this JToken token, Organization organization)
        {
            Assert.Equal(organization.OrganizationId, token.Value<int>("organizationId"));
            Assert.Equal(organization.Name, token.Value<string>("name"));
            Assert.Equal(organization.Description, token.Value<string>("description"));
            Assert.Equal(organization.Url, token.Value<string>("url"));
            Assert.Equal(organization.Phone, token.Value<string>("phone"));
            Assert.Equal(organization.Email, token.Value<string>("email"));
            Assert.Equal(organization.LogoUrl, token.Value<string>("logoUrl"));
            Assert.Equal(organization.LogoBase64, token.Value<string>("logoBase64"));
        }

        public static void CheckStringArray(this JArray array, params string[] roles)
        {
            array.CheckArray((t, r) => Assert.Equal(r, t.ToString()), roles);
        }

        public static JArray CheckArray<T>(this JArray token, Action<JToken, T> f, params T[] values)
        {
            Assert.Equal(values.Length, token.Count);
            for (var i = 0; i < token.Count; ++i)
            {
                f(token[i], values[i]);
            }

            return token;
        }

        public static JArray CheckEmptyArray(this JArray token)
        {
            Assert.Empty(token);
            return token;
        }

        public static JToken CheckEmptyPaging(this JToken token)
        {
            Assert.Equal(1, token.Value<int>("page"));
            Assert.Equal(0, token.Value<int>("total"));
            Assert.Equal(0, token.Value<int>("pages"));
            token.Value<JArray>("data").CheckEmptyArray();
            return token;
        }

        public static JToken CheckPaging<T>(this JToken token, Action<JToken, T> f,
            params T[] data)
        {
            return CheckPaging(token, 1, null, null, f, data);
        }

        public static JToken CheckPaging<T>(this JToken token, int page, int? total, Action<JToken, T> f,
            params T[] data)
        {
            return CheckPaging(token, page, null, total, f, data);
        }

        public static JToken CheckPaging<T>(this JToken token, int page, int? count, int? total, Action<JToken, T> f,
            params T[] data)
        {
            Assert.Equal(page, token.Value<int>("page"));
            if (count.HasValue)
            {
                Assert.Equal(count.Value, token.Value<int>("count"));
            }

            if (total.HasValue)
            {
                Assert.Equal(total.Value, token.Value<int>("total"));
            }

            var arr = token.Value<JArray>("data");
            arr.CheckArray(f, data);
            return token;
        }
    }
}
