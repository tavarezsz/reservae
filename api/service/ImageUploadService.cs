using Reservae.Models.DTOs;

namespace Reservae.Service;

public class ImageUploadService(IWebHostEnvironment environment)
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> ContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    public async Task<UploadedImageDto> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new InvalidUploadException("O arquivo enviado está vazio.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidUploadException("A imagem deve ter no máximo 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType))
            throw new InvalidUploadException(
                "Formato não suportado. Envie uma imagem JPG, PNG ou WebP.");

        if (!await HasValidSignatureAsync(file, extension, cancellationToken))
            throw new InvalidUploadException(
                "O conteúdo do arquivo não corresponde a uma imagem válida.");

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var webRootPath = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadDirectory = Path.Combine(webRootPath, "uploads", "images");
        Directory.CreateDirectory(uploadDirectory);

        var physicalPath = Path.Combine(uploadDirectory, storedFileName);

        try
        {
            await using var destination = new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous);
            await file.CopyToAsync(destination, cancellationToken);
        }
        catch
        {
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            throw;
        }

        return new UploadedImageDto
        {
            Path = $"/uploads/images/{storedFileName}",
            FileName = storedFileName,
            ContentType = contentType,
            Size = file.Length
        };
    }

    public void Delete(string publicPath)
    {
        if (!publicPath.StartsWith('/') || publicPath.StartsWith("//"))
            return;

        var extension = Path.GetExtension(publicPath);
        if (!ContentTypes.ContainsKey(extension))
            return;

        var webRootPath = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var normalizedWebRoot = Path.GetFullPath(webRootPath)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var relativePath = publicPath
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(
            Path.Combine(normalizedWebRoot, relativePath));

        if (!physicalPath.StartsWith(normalizedWebRoot, StringComparison.Ordinal))
            return;

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);
    }

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header, cancellationToken);

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                bytesRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),

            ".webp" =>
                bytesRead >= 12 &&
                header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                header.AsSpan(8, 4).SequenceEqual("WEBP"u8),

            _ => false
        };
    }
}
