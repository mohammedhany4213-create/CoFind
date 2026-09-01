using System.Net;
using System.Net.Http.Json;
using coFind.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace coFind.IntegrationTests;

public class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_And_Refresh_Work()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(
            "Integration Test", email, "01012345678", "Password123!"));

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequest(
            email, "Password123!"));

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var loginResponse = await login.Content.ReadFromJsonAsync<LoginUserResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse!.Token));
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.RefreshToken));

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginResponse.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var refreshResponse = await refresh.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        Assert.NotNull(refreshResponse);
        Assert.False(string.IsNullOrWhiteSpace(refreshResponse!.Token));
        Assert.False(string.IsNullOrWhiteSpace(refreshResponse.RefreshToken));
        Assert.NotEqual(loginResponse.RefreshToken, refreshResponse.RefreshToken);

        var reused = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginResponse.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(
            "Integration Test", email, "01012345678", "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequest(
            email, "WrongPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}