using System.ComponentModel.DataAnnotations;
using Reservae.Models.Enums;

namespace Reservae.Models.DTOs;

public class CreateSpaceDto
{
    [Required, StringLength(500)]
    public required string Address { get; init; }

    [Required, StringLength(200)]
    public required string Title { get; init; }

    [Required, StringLength(4000)]
    public required string Description { get; init; }

    public CategoryEnum? Category { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço não pode ser negativo.")]
    public decimal? PricePerSpot { get; init; }
}
