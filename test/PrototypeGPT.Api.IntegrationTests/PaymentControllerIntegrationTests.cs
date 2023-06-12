using PrototypeGPT.Api.Features.Payment.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace PrototypeGPT.Api.Tests.Features.Payment
{
    public class PaymentControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PaymentControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("/api/payment/create", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"clientSecret\":\"client_secret\"}", json);
        }
    }
}
