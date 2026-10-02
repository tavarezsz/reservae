using Microsoft.EntityFrameworkCore;
using Reservae.Data;

namespace Reservae.Authorization;

public enum ResourceKind { Space, AvailabilityRule, BookableSlot, Booking, SlotCreation }

public interface IResourceOwnershipResolver
{
    Task<ResourceOwnership?> ResolveAsync(ResourceKind kind, int id, CancellationToken cancellationToken);
}

public sealed class ResourceOwnershipResolver(ApplicationDbContext context) : IResourceOwnershipResolver
{
    public Task<ResourceOwnership?> ResolveAsync(ResourceKind kind, int id, CancellationToken cancellationToken)
        => kind switch
        {
            ResourceKind.Space => context.Spaces.AsNoTracking()
                .Where(space => space.Id == id)
                .Select(space => new ResourceOwnership(space.OwnerId, null))
                .SingleOrDefaultAsync(cancellationToken),
            ResourceKind.AvailabilityRule => context.AvailabilityRules.AsNoTracking()
                .Where(rule => rule.Id == id)
                .Select(rule => new ResourceOwnership(rule.Space.OwnerId, null))
                .SingleOrDefaultAsync(cancellationToken),
            ResourceKind.BookableSlot => context.BookableSlots.AsNoTracking()
                .Where(slot => slot.Id == id)
                .Select(slot => new ResourceOwnership(slot.Space.OwnerId, null))
                .SingleOrDefaultAsync(cancellationToken),
            ResourceKind.Booking => context.Bookings.AsNoTracking()
                .Where(booking => booking.Id == id)
                .Select(booking => new ResourceOwnership(booking.BookableSlot.Space.OwnerId, booking.UserBookedId))
                .SingleOrDefaultAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
}
