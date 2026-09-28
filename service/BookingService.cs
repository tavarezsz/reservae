using Microsoft.AspNetCore.Identity;
using Reservae.Models.Interfaces;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Service.Mappers;

namespace Reservae.Service;

public class BookingService(
    IBookingRepository bookingRepository,
    IBookableSlotRepository bookableSlotRepository,
    IBaseRepository<AvailabilityRule> availabilityRuleRepository,
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

    public async Task<BookingDto> CreateBookingAutoAsync(CreateBookingAutoDto dto)
    {
        _ = await userManager.FindByIdAsync(dto.UserBookedId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        BookableSlot slot;

        if (dto.BookableSlotId is int slotId)
        {
            slot = await bookableSlotRepository.GetByIdAsync(slotId)
                ?? throw new KeyNotFoundException("Horário não encontrado.");

            if (slot.SpaceId != dto.SpaceId)
                throw new ArgumentException("O horário não pertence ao espaço informado.");
        }
        else if (dto.AvailabilityRuleId is int ruleId)
        {
            var rule = await availabilityRuleRepository.GetByIdAsync(ruleId)
                ?? throw new KeyNotFoundException("Regra não encontrada.");

            if (rule.SpaceId != dto.SpaceId)
                throw new ArgumentException("A regra não pertence ao espaço informado.");

            rule.ValidateOccurrence(dto.StartsAt, dto.EndsAt);

            slot = await bookableSlotRepository.GetByRuleOccurrenceAsync(
                    rule.Id,
                    dto.StartsAt)
                ?? await bookableSlotRepository.AddAsync(
                    BookableSlot.FromRule(
                        rule.Id,
                        rule.SpaceId,
                        dto.StartsAt,
                        dto.EndsAt,
                        customPricePerSpot: null,
                        rule.Capacity));
        }
        else
        {
            throw new ArgumentException(
                "Um horário avulso precisa ser criado antes da reserva e possuir um BookableSlotId.");
        }

        var booking = new Booking(
            slot.Id,
            dto.UserBookedId,
            BookingStatusEnum.Confirmado,
            dto.Quantity);
        var createdBooking = await bookingRepository.AddAsync(booking);

        return createdBooking.ToDto();
    }

}
