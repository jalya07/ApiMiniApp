namespace WebApplication2.Service;

public class FileUploadService:IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
 
    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }
 
    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");
 
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);
 
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);
 
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
 
        // Return a web-accessible relative path
        return $"/uploads/{subFolder}/{fileName}";
    }
 
    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
 
        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}