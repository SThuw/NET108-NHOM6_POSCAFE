using System.ComponentModel.DataAnnotations;

namespace CafePOSManagement.Web.Utils;

public static class FileUtil
{
    public static async Task<string> SaveImageAsync(IFormFile file, string webRoot, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0 || file.Length > 2 * 1024 * 1024)
            throw new ValidationException("Ảnh phải có dung lượng từ 1 byte đến 2 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            throw new ValidationException("Chỉ chấp nhận ảnh JPG, PNG hoặc WebP.");

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var read = await input.ReadAsync(header, cancellationToken);
        var valid = extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => read >= 12 && header[..4].SequenceEqual("RIFF"u8.ToArray()) && header[8..12].SequenceEqual("WEBP"u8.ToArray()),
            _ => false
        };
        if (!valid) throw new ValidationException("Nội dung tệp không khớp định dạng ảnh.");

        var directory = Path.Combine(webRoot, "images", "uploads");
        Directory.CreateDirectory(directory);
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write);
        await output.WriteAsync(header.AsMemory(0, read), cancellationToken);
        await input.CopyToAsync(output, cancellationToken);
        return $"/images/uploads/{name}";
    }
}
