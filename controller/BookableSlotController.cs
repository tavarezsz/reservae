using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[ApiController]
[Route("api/bookable-slots")]
public class BookableSlotController(BookableSlotService bookableSlotService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<BookableSlotDTO>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await bookableSlotService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookableSlotDTO>> GetById(int id)
    {
        var result = await bookableSlotService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<BookableSlotDTO>(StatusCodes.Status201Created)]
    public async Task<ActionResult<BookableSlotDTO>> Create(
        [FromBody] CreateBookableSlotDTO dto)
    {
        var result = await bookableSlotService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookableSlotDTO>> Update(
        int id,
        [FromBody] UpdateBookableSlotDTO dto)
    {
        var result = await bookableSlotService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(int id)
    {
        await bookableSlotService.DeleteAsync(id);
        return NoContent();
    }
}
