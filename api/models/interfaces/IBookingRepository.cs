namespace Reservae.Models.Interfaces;

using Reservae.Models.DTOs;

public interface IBookingRepository : IBaseRepository<Booking>
{
    Task<PagedResponseDto<Booking>> GetBySpaceIdAsync(
        int spaceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResponseDto<Booking>> GetByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
