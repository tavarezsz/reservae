using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[ApiController]
[Route("api/bookings")]

public class BookingController(BookingService bookingService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetByUserIdPaged(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 10)
    {
        var result = await bookingService.GetPagedByUserIdAsync(page, pagesize, userId);
        return Ok(result);
    }

    [HttpGet("space/{spaceId:int}")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetBySpaceIdPaged(
        int spaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 10)
    {
        var result = await bookingService.GetPagedBySpaceIdAsync(page, pagesize, spaceId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var result = await bookingService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("create-manual")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingDto>> Create(
        [FromBody] CreateBookingDto dto)
    {
        var result  = await bookingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingDto>> Update(
        int id,
        [FromBody] UpdateBookingDto dto
    )
    {
        var result = await bookingService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(int id)
    {
        await bookingService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("create-auto")]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingDto>> CreateAuto(
        [FromBody] CreateBookingAutoDto dto
    )
    {
        var result = await bookingService.CreateBookingAutoAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
