using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using json_comparison;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api.IntegrationTests
{
    [TestClass]
    public class JsonComparerControllerTests
    {
        private IMemoryCache _cache;
        public JsonComparerControllerTests()
        { 
           _cache = _factory.Services.GetService<IMemoryCache>();        
        }
        private static WebApplicationFactory<Startup> _factory;
        private static readonly Uri BASE_URL = new Uri("https://localhost:5001/");
        private const string ID = "9245fe4a-d402-451c-b9ed-9c1a04247482";


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _factory = new WebApplicationFactory<Startup>(); 
        }

        [TestMethod]
        public async Task Should_Hit_API_And_Set_Left_JSON_Object_In_Memory()
        { 
            
             // Arrange 
            var client = _factory.CreateClient();
            client.BaseAddress = BASE_URL;

            //~ Left Input ~//
            string base64String = "eyJBZ2VudCI6IjEyODkiLCJCZW5lZmljaWFyeSI6eyJNYWluIjoidHJ1ZSIsIlZhbHVlcyI6WyIxIiwiMiIsIjMiLCI0IiwiNSJdfSwiQmlsbGluZ0RhdGUiOiIyMDIxLTA0LTA0VDEzOjMzOjAzLjk2OVoiLCJCaWxsaW5nSW52b2ljZSI6IjkxNDU0OCIsIkNsYWltIjp7IkN1c3RvbWVyIjoiMTEyMjMzIiwiUHJvZHVjdHMiOlsib25lIiwidHdvIiwidGhyZWUiXX19";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(base64String), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync($"v1/diff/{ID}/Left", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var result = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(result.ToString().Contains("Your input has been successfully saved!"));

            //~ Check if it had been set in Memory ~//
            string value; 
            _cache.TryGetValue<string>("LEFT_" + ID, out value);

            Assert.AreEqual(value, base64String);
        }

        [TestMethod]
        public async Task Should_Hit_API_And_Set_Right_JSON_Object_In_Memory()
        {

            // Arrange 
            var client = _factory.CreateClient();
            client.BaseAddress = BASE_URL;

            //~ Right Input ~//
            string base64String = "eyJBZ2VudCI6IjEyODkiLCJCZW5lZmljaWFyeSI6eyJNYWluIjoidHJ1ZSIsIlZhbHVlcyI6WyI1IiwiNiIsIjciXX0sIkJpbGxpbmdEYXRlIjoiMjAyMS0wNC0wNFQxMzozMzowMy45NjlaIiwiQmlsbGluZ0ludm9pY2UiOiI5MTQ1NDgiLCJDbGFpbSI6eyJDdXN0b21lciI6IjMzIiwiUHJvZHVjdHMiOlsib25lIiwidGhyZWUiLCJzaXgiXX19";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(base64String), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync($"v1/diff/{ID}/Right", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(result.ToString().Contains("Your input has been successfully saved!"));

            //~ Check if it had been set in Memory ~//
            string value;
            _cache.TryGetValue<string>("RIGHT_" + ID, out value);

            Assert.AreEqual(value, base64String);
        }
        [TestMethod]
        public async Task Should_Hit_API_And_Get_The_Diff_Of_Left_And_Right_JSON_Objects()
        {

            // Arrange 
            var client = _factory.CreateClient();
            client.BaseAddress = BASE_URL;
 
            HttpContent content = new StringContent(JsonConvert.SerializeObject("m"), Encoding.UTF8, "application/json");
            var expected_diff = JObject.Parse(@"{""Beneficiary"":{""Values"":{""Offset"":[""1"",""2"",""3"",""4"",""6"",""7""],""Length"":6}},""Claim"":{""Customer"":{""Offset"":[""112233"",""33""],""Length"":2},""Products"":{""Offset"":[""two"",""six""],""Length"":2}}}");

            // Act
            var response = await client.PostAsync($"v1/diff/{ID}", null);

            // Assert
            if(response.StatusCode == HttpStatusCode.BadRequest)
            {
                var result = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(result.ToString().Contains("Either Left or Right Object is not set properly!"));
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {  
                var result = await response.Content.ReadAsStringAsync(); 
                var res = JsonConvert.DeserializeObject<JObject>(result);

                var message = res.SelectToken("message");
                var diff = res.SelectToken("data");

                Assert.AreEqual(message.ToString(), "OBJECTS ARE NOT EQUAL");
                Assert.AreEqual(diff.ToString(), expected_diff.ToString());
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        } 
    } 
}