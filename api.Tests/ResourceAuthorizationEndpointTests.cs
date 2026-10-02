using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Reservae.Data;

namespace Reservae.Tests;

public sealed class ResourceAuthorizationEndpointTests(AuthorizationApiFactory factory) : IClassFixture<AuthorizationApiFactory>
{
    [Theory]
    [InlineData("PUT", "/api/spaces/1")]
    [InlineData("PUT", "/api/availability-rule/1")]
    [InlineData("PUT", "/api/bookable-slots/1")]
    [InlineData("DELETE", "/api/bookable-slots/1")]
    [InlineData("GET", "/api/bookings/1")]
    [InlineData("GET", "/api/bookings/space/1")]
    [InlineData("PUT", "/api/bookings/1")]
    [InlineData("DELETE", "/api/bookings/1")]
    public async Task UnrelatedUserCannotAccessProtectedResource(string method, string path)
    {
        using var client = factory.ClientFor("stranger");
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(new { });
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("PUT", "/api/spaces/1")]
    [InlineData("PUT", "/api/availability-rule/1")]
    [InlineData("PUT", "/api/bookable-slots/1")]
    [InlineData("DELETE", "/api/bookable-slots/1")]
    [InlineData("GET", "/api/bookings/1")]
    public async Task AnonymousUserIsChallenged(string method, string path)
    {
        using var client = factory.ClientFor(null);
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(new { });
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("client")]
    public async Task ClientAndAdvertiserCanReadBooking(string userId)
    {
        using var client = factory.ClientFor(userId);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/bookings/1")).StatusCode);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AdvertiserCannotModifyClientBooking(string method)
    {
        using var client = factory.ClientFor("owner");
        using var request = new HttpRequestMessage(new HttpMethod(method), "/api/bookings/1");
        if (method == "PUT") request.Content = JsonContent.Create(new { });
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task ClientCanDeleteOwnBooking()
    {
        using var client = factory.ClientFor("client");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/bookings/2")).StatusCode);
    }

    [Fact]
    public async Task OnlySpaceOwnerCanReadReceivedBookings()
    {
        using var owner = factory.ClientFor("owner");
        using var client = factory.ClientFor("client");
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync("/api/bookings/space/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/bookings/space/1")).StatusCode);
    }

    [Fact]
    public async Task SpaceOwnerCanUpdateSpace()
    {
        using var owner = factory.ClientFor("owner");
        var response = await owner.PutAsJsonAsync("/api/spaces/1", new { title = "Updated by owner" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/availability-rule/1")]
    [InlineData("/api/bookable-slots/1")]
    public async Task SpaceOwnerCanUpdateSchedules(string path)
    {
        using var owner = factory.ClientFor("owner");
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync(path, new { })).StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    public async Task SpaceOwnerCanCreateStandaloneOrRuleSlot(int? ruleId)
    {
        using var owner = factory.ClientFor("owner");
        var response = await owner.PostAsJsonAsync("/api/bookable-slots", new
        {
            spaceId = ruleId is null ? (int?)1 : null, availabilityRuleId = ruleId, capacity = 10,
            startsAt = "2030-01-07T10:00:00Z", endsAt = "2030-01-07T11:00:00Z"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UnrelatedUpdateDoesNotChangeSpace()
    {
        using var stranger = factory.ClientFor("stranger");
        var response = await stranger.PutAsJsonAsync("/api/spaces/1", new { title = "Forbidden change" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.NotEqual("Forbidden change", scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Spaces.Find(1)!.Title);
    }

    [Fact]
    public async Task CreatingRuleRequiresOwnershipOfSpaceInBody()
    {
        using var client = factory.ClientFor("stranger");
        var response = await client.PostAsJsonAsync("/api/availability-rule", new
        {
            spaceId = 1, startTime = "09:00:00", endTime = "12:00:00",
            validFrom = "2030-01-07T00:00:00Z", validUntil = "2030-02-07T00:00:00Z", capacity = 10, slotDurationMinutes = 60
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(1, null)]
    [InlineData(2, 1)]
    [InlineData(null, 1)]
    public async Task SlotCreationChecksActualSpaceOrRuleOwner(int? spaceId, int? ruleId)
    {
        using var client = factory.ClientFor("stranger");
        var response = await client.PostAsJsonAsync("/api/bookable-slots", new
        {
            spaceId, availabilityRuleId = ruleId, capacity = 10,
            startsAt = "2030-01-07T10:00:00Z", endsAt = "2030-01-07T11:00:00Z"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadIsRejectedBeforeFileIsSaved()
    {
        using var client = factory.ClientFor("stranger");
        using var body = new MultipartFormDataContent();
        body.Add(new StringContent("1"), "spaceId");
        body.Add(new ByteArrayContent([1, 2, 3]), "file", "invalid.jpg");
        var response = await client.PostAsync("/api/uploads/images", body);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/spaces/1")]
    [InlineData("/api/availability-rule")]
    [InlineData("/api/availability-rule/1")]
    [InlineData("/api/availability-rule/space/1")]
    [InlineData("/api/bookable-slots")]
    [InlineData("/api/bookable-slots/1")]
    [InlineData("/api/bookable-slots/space/1/standalone")]
    public async Task ResourceQueriesRemainPublic(string path)
    {
        using var client = factory.ClientFor(null);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("PUT", "/api/spaces/999")]
    [InlineData("PUT", "/api/availability-rule/999")]
    [InlineData("GET", "/api/bookings/999")]
    public async Task MissingResourceReturnsNotFound(string method, string path)
    {
        using var client = factory.ClientFor("owner");
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "PUT") request.Content = JsonContent.Create(new { });
        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(request)).StatusCode);
    }
}
