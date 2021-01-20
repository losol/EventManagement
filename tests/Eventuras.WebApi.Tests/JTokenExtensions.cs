using System;
using Eventuras.Domain;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Eventuras.WebApi.Tests
{
    public static class JTokenExtensions
    {
        public static void CheckRegistration(this JToken token, Registration registration)
        {
            Assert.Equal(registration.RegistrationId, token.Value<int>("registrationId"));
            Assert.Equal(registration.EventInfoId, token.Value<int>("eventId"));
            Assert.Equal(registration.UserId, token.Value<string>("userId"));
            Assert.Equal((int)registration.Status, token.Value<int>("status"));
            Assert.Equal((int)registration.Type, token.Value<int>("type"));
            Assert.Equal(registration.Notes, token.Value<string>("notes"));
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

        public static JToken CheckPaging<T>(this JToken token, int page, int? total, Action<JToken, T> f,
            params T[] data)
        {
            return CheckPaging(token, page, null, total, f, data);
        }

        public static JToken CheckPaging<T>(this JToken token, int page, int? count, int? total, Action<JToken, T> f, params T[] data)
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
