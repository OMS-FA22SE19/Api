using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces
{
    public interface IUploadService
    {
        Task<string> UploadAsync(IFormFile file, string folder = "files");
    }
}
