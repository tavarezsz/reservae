using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[Authorize]
[ApiController]
[Route("api/bookings")]

public class BookingController(BookingService bookingService, UserManager<User> userManager) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResponseDto<BookingDto>>> GetByUserIdPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 10)
    {
        var currentUserId = userManager.GetUserId(User);
        if (currentUserId is null) return Unauthorized();
        var result = await bookingService.GetPagedByUserIdAsync(page, pagesize, currentUserId);
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
        var currentUserId = userManager.GetUserId(User);
        if (currentUserId is null) return Unauthorized();
        var result = await bookingService.CreateAsync(dto, currentUserId);
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
        var currentUserId = userManager.GetUserId(User);
        if (currentUserId is null) return Unauthorized();
        var result = await bookingService.CreateBookingAutoAsync(dto, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
