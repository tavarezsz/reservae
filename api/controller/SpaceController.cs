using Microsoft.AspNetCore.Mvc;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[ApiController]
[Route("api/spaces")]
public class SpaceController(SpaceService spaceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<SpaceDTO>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await spaceService.GetPagedAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpaceDTO>> GetById(int id)
    {
        var result = await spaceService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<SpaceDTO>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SpaceDTO>> Create(
        [FromBody] CreateSpaceDto dto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await spaceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpaceDTO>> Update(
        int id,
        [FromBody] UpdateSpaceDto updateSpaceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await spaceService.UpdateAsync(id, updateSpaceDto);
        return Ok(result);
    }

    [HttpGet("{spaceId}/availability")]
    public async Task<ActionResult<IReadOnlyList<AvailableSlotDto>>> GetAvailability(
        int spaceId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] DayOfTheWeekEnum? dayOfTheWeek,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var slots = await spaceService.GetAvailabilityAsync(
            spaceId,
            fromDate,
            toDate,
            dayOfTheWeek,
            cancellationToken
        );

        return Ok(slots);
    }
}
