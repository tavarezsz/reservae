using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Reservae.Models.DTOs;

public class UploadImageRequest
{
    [Required, Range(1, int.MaxValue)]
    [FromForm(Name = "spaceId")]
    public int SpaceId { get; init; }

    [Required]
    [FromForm(Name = "file")]
    public required IFormFile File { get; init; }
}
