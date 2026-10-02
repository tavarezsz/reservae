using Microsoft.AspNetCore.Authorization;

namespace Reservae.Authorization;

public static class ResourcePolicies
{
    public const string SpaceOwner = "Resource.SpaceOwner";
    public const string BookingReader = "Resource.BookingReader";
    public const string BookingUser = "Resource.BookingUser";

    public static void Configure(AuthorizationOptions options)
    {
        AddPolicy(options, SpaceOwner, ResourceAccess.SpaceOwner);
        AddPolicy(options, BookingReader, ResourceAccess.BookingReader);
        AddPolicy(options, BookingUser, ResourceAccess.BookingUser);
    }

    private static void AddPolicy(AuthorizationOptions options, string name, ResourceAccess access)
        => options.AddPolicy(name, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new ResourceOwnershipRequirement(access)));
}

public enum ResourceAccess { SpaceOwner, BookingReader, BookingUser }

public sealed record ResourceOwnership(string SpaceOwnerId, string? BookingUserId = null);

public sealed record ResourceOwnershipRequirement(ResourceAccess Access) : IAuthorizationRequirement;
