using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;
using System.ComponentModel.DataAnnotations;
using Reservae.Authorization;

namespace Reservae.Controllers;


[ApiController]
[Route("api/availability-rule")]

public class AvailabilityRuleController(AvailabilityRuleService availabilityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<AvailabilityRuleDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await availabilityService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AvailabilityRuleDto>> GetById(int id)
    {
        var result = await availabilityService.GetByIdAsync(id);

        return Ok(result);
    }


    [HttpGet("space/{spaceId:int}")]
    public async Task<ActionResult<PagedResponseDto<AvailabilityRuleDto>>> GetForSpace(
        int spaceId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
        => Ok(await availabilityService.GetForSpaceAsync(spaceId, page, pageSize));

    [ResourceAuthorize(ResourceKind.Space, "dto.SpaceId")]
    [HttpPost]
    [ProducesResponseType<AvailabilityRuleDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AvailabilityRuleDto>> Create(
        [FromBody] CreateAvailabilityRuleDTO dto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await availabilityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [ResourceAuthorize(ResourceKind.AvailabilityRule, "id")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AvailabilityRuleDto>> Update(
        int id,
        [FromBody] UpdateAvailabilityRuleDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await availabilityService.UpdateAsync(id, dto);

        return Ok(result);
    }
}
