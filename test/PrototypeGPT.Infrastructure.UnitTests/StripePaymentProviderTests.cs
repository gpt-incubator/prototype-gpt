using PrototypeGPT.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using Xunit;

namespace PrototypeGPT.Infrastructure.Tests
{
    public class StripePaymentProviderTests
    {
        [Fact]
        public async Task CreateAsync_Should_Return_ClientSecret()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<StripeOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new StripeOptions { SecretKey = "test_secret_key" });

            var mockPaymentIntentService = new Mock<PaymentIntentService>();
            mockPaymentIntentService.Setup(x => x.CreateAsync(It.IsAny<PaymentIntentCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PaymentIntent { ClientSecret = "test_client_secret" }));

            var provider = new StripePaymentProvider(mockOptions.Object)
            {
                PaymentIntentService = mockPaymentIntentService.Object
            };

            // Act
            var result = await provider.CreateAsync(CancellationToken.None);

            // Assert
            Assert.Equal("test_client_secret", result);
        }
    }
}
