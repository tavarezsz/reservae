using Reservae.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Reservae.Repository.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResponseDto<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponseDto<T>
        {
            Items = items,
            TotalCount = total,
        };
    }
}
