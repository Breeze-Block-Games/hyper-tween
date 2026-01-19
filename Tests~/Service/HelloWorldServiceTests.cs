using System.Net.Http.Json;
using BreezeBlockGames.PackageBase.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BreezeBlockGames.PackageBase.Tests.Service
{
    public class HelloWorldServiceTests
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp] 
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown] 
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }

        [Test]
        public async Task Get_Hello_Returns_Ok_And_Payload()
        {
            var client = _factory.CreateClient();

            const string name = "Dave";
            const string ipAddress = "127.0.0.1";
        
            var req = new HttpRequestMessage(HttpMethod.Get, $"/v1/hello?Name={name}");
            req.Headers.TryAddWithoutValidation("X-Forwarded-For", ipAddress);

            var resp = await client.SendAsync(req);
        
            // Grab the raw body string no matter what
            var raw = await resp.Content.ReadAsStringAsync();

            // Fail fast if not 2xx, but include body in failure message
            Assert.That(resp.IsSuccessStatusCode, Is.True, 
                $"Expected success but got {resp.StatusCode} ({(int)resp.StatusCode}). Response body:\n{raw}");

            var body = await resp.Content.ReadFromJsonAsync<HelloWorldResponse>();
        
            Assert.That(body, Is.Not.Null);
            Assert.That(body.Message, Does.Contain(name));
            Assert.That(body.Message, Does.Contain(ipAddress));
        }

        [Test]
        public async Task Get_Hello_Requires_Name_Returns_400()
        {
            var client = _factory.CreateClient();
        
            // missing name
            var resp = await client.GetAsync("/v1/hello");
        
            Assert.That((int)resp.StatusCode, Is.EqualTo(400));
        }
    }
}