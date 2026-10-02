using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Service;
using System.ComponentModel.DataAnnotations;
using Reservae.Authorization;

namespace Reservae.Controllers;

[ApiController]
[Route("api/spaces")]
public class SpaceController(SpaceService spaceService, UserManager<User> userManager) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponseDto<SpaceDTO>>> Search(
        [FromQuery, Required, StringLength(200, MinimumLength = 2)] string term,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await spaceService.SearchAsync(
            term,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

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

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResponseDto<SpaceDTO>>> GetForOwner(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        var currentUserId = userManager.GetUserId(User);
        if (currentUserId is null) return Unauthorized();
        return Ok(await spaceService.GetForOwnerAsync(currentUserId, page, pageSize));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType<SpaceDTO>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SpaceDTO>> Create(
        [FromBody] CreateSpaceDto dto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = userManager.GetUserId(User);
        if (currentUserId is null) return Unauthorized();

        var result = await spaceService.CreateAsync(dto, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [ResourceAuthorize(ResourceKind.Space, "id")]
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

    [HttpGet("{spaceId:int}/availability-limit")]
    public async Task<ActionResult<SpaceAvailabilityLimitDto>> GetAvailabilityLimit(
        int spaceId,
        CancellationToken cancellationToken)
    {
        var limit = await spaceService.GetAvailabilityLimitAsync(spaceId, cancellationToken);
        return Ok(limit);
    }
}
