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
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await bookableSlotService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await bookableSlotService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookableSlotDTO dto)
    {
        var result = await bookableSlotService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBookableSlotDTO dto)
    {
        var result = await bookableSlotService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await bookableSlotService.DeleteAsync(id);
        return NoContent();
    }
}
