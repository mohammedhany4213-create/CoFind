using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using coFind.Application.DTOs;
using Xunit;

namespace coFind.IntegrationTests;

public sealed class OffersAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OffersAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOffer_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/offers", CreateRequest());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OwnerCanCreateUpdateAndDeleteOffer_AndOtherUserCannotModifyIt()
    {
        var ownerToken = await RegisterAndLoginAsync();
        var otherUserToken = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var create = await _client.PostAsJsonAsync("/api/offers", CreateRequest());
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreateOfferResponse>();
        Assert.NotNull(created);

        var offerId = created!.OfferId;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);
        var updateByOther = await _client.PutAsJsonAsync($"/api/offers/{offerId}", UpdateRequest());
        Assert.Equal(HttpStatusCode.Forbidden, updateByOther.StatusCode);

        var deleteByOther = await _client.DeleteAsync($"/api/offers/{offerId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteByOther.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var updateByOwner = await _client.PutAsJsonAsync($"/api/offers/{offerId}", UpdateRequest());
        Assert.Equal(HttpStatusCode.OK, updateByOwner.StatusCode);

        var deleteByOwner = await _client.DeleteAsync($"/api/offers/{offerId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteByOwner.StatusCode);
    }

    [Fact]
    public async Task GetOffer_IsPublic_ButDeletedOfferIsNotVisible()
    {
        var ownerToken = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var create = await _client.PostAsJsonAsync("/api/offers", CreateRequest());
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreateOfferResponse>();
        Assert.NotNull(created);

        _client.DefaultRequestHeaders.Authorization = null;
        var get = await _client.GetAsync($"/api/offers/{created!.OfferId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var delete = await _client.DeleteAsync($"/api/offers/{created.OfferId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var getDeleted = await _client.GetAsync($"/api/offers/{created.OfferId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
    }

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"offer-test-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(
            "Offer Test", email, "Password123!", "01012345678"));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequest(email, "Password123!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var response = await login.Content.ReadFromJsonAsync<LoginUserResponse>();
        Assert.NotNull(response);
        return response!.Token;
    }

    private static CreateOfferRequest CreateRequest() => new(
        "Backend Engineer",
        "Build and maintain backend services for the platform.",
        "Backend Engineer",
        new List<string> { "C#", ".NET" },
        "Software",
        true,
        "Cairo");

    private static UpdateOfferRequest UpdateRequest() => new(
        "Senior Backend Engineer",
        "Build and maintain backend services and APIs for the platform.",
        "Senior Backend Engineer",
        new List<string> { "C#", ".NET", "SQL" },
        "Software",
        true,
        "Cairo");
}