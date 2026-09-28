using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[ApiController]
[Route("api/bookings")]

public class BookingController(BookingService bookingService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserIdPaged(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 10)
    {
        var result = await bookingService.GetPagedByUserIdAsync(page, pagesize, userId);
        return Ok(result);
    }

    [HttpGet("space/{spaceId:int}")]
    public async Task<IActionResult> GetBySpaceIdPaged(
        int spaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pagesize = 10)
    {
        var result = await bookingService.GetPagedBySpaceIdAsync(page, pagesize, spaceId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var result  = await bookingService.CreateAsync(dto);
        return CreatedAtAction(nameof(Create), new {id = result.Id}, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBookingDto dto
    )
    {
        var result = await bookingService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await bookingService.DeleteAsync(id);
        return NoContent();
    }
}
