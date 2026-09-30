using Reservae.Models.DTOs;

namespace Reservae.Models.Interfaces;

public interface ISpaceRepository : IBaseRepository<Space>
{
    Task<PagedResponseDto<Space>> GetActivePagedAsync(int page, int pageSize);
    Task<PagedResponseDto<Space>> GetForOwnerAsync(string ownerId, int page, int pageSize);
    Task<PagedResponseDto<Space>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
