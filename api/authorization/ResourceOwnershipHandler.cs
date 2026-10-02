using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Reservae.Authorization;

public sealed class ResourceOwnershipHandler(IOptions<IdentityOptions> identityOptions)
    : AuthorizationHandler<ResourceOwnershipRequirement, ResourceOwnership>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnershipRequirement requirement,
        ResourceOwnership resource)
    {
        var userId = context.User.FindFirst(identityOptions.Value.ClaimsIdentity.UserIdClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(userId) || !context.User.Identities.Any(identity => identity.IsAuthenticated))
            return Task.CompletedTask;

        var isSpaceOwner = userId == resource.SpaceOwnerId;
        var isBookingUser = userId == resource.BookingUserId;
        var allowed = requirement.Access switch
        {
            ResourceAccess.SpaceOwner => isSpaceOwner,
            ResourceAccess.BookingReader => isSpaceOwner || isBookingUser,
            ResourceAccess.BookingUser => isBookingUser,
            _ => false
        };

        if (allowed) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
