using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;


[ApiController]
[Route("api/availability-rule")]

public class AvailabilityRuleController(AvailabilityRuleService availabilityService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await availabilityService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await availabilityService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAvailabilityRuleDTO dto
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await availabilityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAvailabilityRuleDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await availabilityService.UpdateAsync(id, dto);

        return Ok(result);
    }
}
