using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Reservae.Authorization;

namespace Reservae.Tests;

public class ResourceOwnershipHandlerTests
{
    [Theory]
    [InlineData(ResourceAccess.SpaceOwner, "owner", true)]
    [InlineData(ResourceAccess.SpaceOwner, "client", false)]
    [InlineData(ResourceAccess.SpaceOwner, "stranger", false)]
    [InlineData(ResourceAccess.BookingReader, "owner", true)]
    [InlineData(ResourceAccess.BookingReader, "client", true)]
    [InlineData(ResourceAccess.BookingReader, "stranger", false)]
    [InlineData(ResourceAccess.BookingUser, "owner", false)]
    [InlineData(ResourceAccess.BookingUser, "client", true)]
    [InlineData(ResourceAccess.BookingUser, "stranger", false)]
    public async Task EnforcesOwnershipAndBookingPermissions(ResourceAccess access, string userId, bool allowed)
    {
        var requirement = new ResourceOwnershipRequirement(access);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, new ResourceOwnership("owner", "client"));
        await new ResourceOwnershipHandler(Options.Create(new IdentityOptions())).HandleAsync(context);
        Assert.Equal(allowed, context.HasSucceeded);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task DoesNotAuthorizeWithoutAuthenticationAndUserId(bool authenticated, bool includeUserId)
    {
        var requirement = new ResourceOwnershipRequirement(ResourceAccess.SpaceOwner);
        var claims = includeUserId ? new[] { new Claim(ClaimTypes.NameIdentifier, "owner") } : [];
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null));
        var context = new AuthorizationHandlerContext([requirement], user, new ResourceOwnership("owner"));
        await new ResourceOwnershipHandler(Options.Create(new IdentityOptions())).HandleAsync(context);
        Assert.False(context.HasSucceeded);
    }
}
