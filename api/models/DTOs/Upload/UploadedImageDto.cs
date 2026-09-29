namespace Reservae.Models.DTOs;

public class UploadedImageDto
{
    public required string Path { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
}
