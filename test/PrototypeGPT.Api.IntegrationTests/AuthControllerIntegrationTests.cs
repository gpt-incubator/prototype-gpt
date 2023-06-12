using PrototypeGPT.Api.Features.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace PrototypeGPT.Api.IntegrationTests;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SignIn_Endpoint_CalledMoreThan100TimesInOneMinute_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var tasks = new List<Task<HttpResponseMessage>>();

        for (var i = 0; i < 120; i++)
        {
            tasks.Add(client.PostAsJsonAsync("/api/auth/signin", new SignInDto("myusername", "mypassword")));
        }

        await Task.WhenAll(tasks);

        var responseCodes = tasks.Select(t => t.Result.StatusCode);

        Assert.Equal(10, responseCodes.Count(c => c == HttpStatusCode.NotFound));
        Assert.Equal(110, responseCodes.Count(c => c == HttpStatusCode.TooManyRequests));
    }
}
