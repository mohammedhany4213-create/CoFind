using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using coFind.Application.DTOs;
using Xunit;

namespace coFind.IntegrationTests;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_Refresh_And_Logout_Work()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(
            "Integration Test", email, "Password123!", "01012345678"));

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

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshResponse.Token);
        var logout = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(refreshResponse.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var afterLogout = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(refreshResponse.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest("invalid-refresh-token"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(
            "Integration Test", email, "Password123!", "01012345678"));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequest(
            email, "WrongPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}