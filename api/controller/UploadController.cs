using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservae.Models.DTOs;
using Reservae.Service;

namespace Reservae.Controllers;

[Authorize]
[ApiController]
[Route("api/uploads")]
public class UploadController(
    ImageUploadService imageUploadService,
    SpaceService spaceService) : ControllerBase
{
    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType<SpaceDTO>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SpaceDTO>> UploadImage(
        [FromForm] UploadImageRequest request,
        CancellationToken cancellationToken)
    {
        var currentSpace = await spaceService.GetByIdAsync(request.SpaceId);

        var uploadedImage = await imageUploadService.UploadAsync(
            request.File,
            cancellationToken);

        try
        {
            var space = await spaceService.ChangeCoverImageAsync(
                request.SpaceId,
                uploadedImage.Path);

            if (currentSpace.CoverImagePath is not null &&
                currentSpace.CoverImagePath != uploadedImage.Path)
            {
                imageUploadService.Delete(currentSpace.CoverImagePath);
            }

            return Ok(space);
        }
        catch
        {
            imageUploadService.Delete(uploadedImage.Path);
            throw;
        }
    }
}
