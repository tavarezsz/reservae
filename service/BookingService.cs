using Microsoft.AspNetCore.Identity;
using Reservae.Models.Interfaces;
using Reservae.Models;
using Reservae.Modesl.DTOs;
using Reservae.Models.DTOs;
using Reservae.Service.Mappers;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Reservae.Service;

public class BookingService(
    IBookingRepository bookingRepository,
    IBaseRepository<BookableSlot> bookableSlotRepository,
    UserManager<User> userManager
)
{
    public async Task<PagedResponseDto<BookingDto>> GetPagedByUserIdAsync(int page, int pageSize, string userId)
    {
        var paged = await bookingRepository.GetByUserIdAsync(userId, page, pageSize);
        return new PagedResponseDto<BookingDto>
        {
            Items = paged.Items.Select(booking => booking.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

        public async Task<PagedResponseDto<BookingDto>> GetPagedBySpaceIdAsync(int page, int pageSize, int spaceId)
    {
        var paged = await bookingRepository.GetBySpaceIdAsync(spaceId, page, pageSize);
        return new PagedResponseDto<BookingDto>
        {
            Items = paged.Items.Select(booking => booking.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await bookingRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Agendamento não encontrado");
        return booking.ToDto();
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        _ = await bookableSlotRepository.GetByIdAsync(dto.BookableSlotId) ?? throw new KeyNotFoundException("Horário não encontrado");
        var user = await userManager.FindByIdAsync(dto.UserBookedId) ?? throw new KeyNotFoundException("Usuário não encontrado");

        var createdBooking = await bookingRepository.AddAsync(dto.ToEntity());
        return createdBooking.ToDto();

    }

    public async Task<BookingDto> UpdateAsync(int id, UpdateBookingDto dto)
    {
        var booking = await bookingRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Agendamento não encontrado");
        BookingMapper.ApplyUpdate(dto, booking);
        await bookingRepository.UpdateAsync(booking);

        return booking.ToDto();
        
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await bookingRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Agendamento não encontrado");
        await bookingRepository.DeleteAsync(id);
    }

}
