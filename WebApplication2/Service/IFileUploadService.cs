namespace WebApplication2.Service;

public interface IFileUploadService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    void DeleteFile(string? relativePath);
}