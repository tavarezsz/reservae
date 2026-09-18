using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Repository.Extensions;

namespace Reservae.Repository;

public class AvailabilityRuleRepository(ApplicationDbContext context)
    : BaseRepository<AvailabilityRule>(context)
{
    public override async Task<AvailabilityRule?> GetByIdAsync(int id)
        => await DbSet
            .Include(rule => rule.Space)
            .FirstOrDefaultAsync(rule => rule.Id == id);

    public override async Task<PagedResponseDto<AvailabilityRule>> GetPagedAsync(
        int page,
        int pageSize)
        => await DbSet
            .Include(rule => rule.Space)
            .OrderBy(rule => rule.CreatedAt)
            .ToPagedAsync(page, pageSize);
}
