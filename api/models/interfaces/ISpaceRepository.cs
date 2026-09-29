using Reservae.Models.DTOs;

namespace Reservae.Models.Interfaces;

public interface ISpaceRepository : IBaseRepository<Space>
{
    Task<PagedResponseDto<Space>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
