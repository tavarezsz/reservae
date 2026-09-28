using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Repository.Extensions;

namespace Reservae.Repository;

public class BookableSlotRepository(ApplicationDbContext context)
    : BaseRepository<BookableSlot>(context)
{
    public override async Task<BookableSlot?> GetByIdAsync(int id)
        => await DbSet
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .FirstOrDefaultAsync(slot => slot.Id == id);

    public override async Task<PagedResponseDto<BookableSlot>> GetPagedAsync(
        int page,
        int pageSize)
        => await DbSet
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .OrderBy(slot => slot.StartsAt)
            .ToPagedAsync(page, pageSize);
}
