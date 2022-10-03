using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class ApplicationUploadService : IUploadService
    {
        public async Task<string> UploadAsync(IFormFile file, string folder = "files")
        {
            string filePath = Path.Combine(folder, Path.GetRandomFileName());

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
            return filePath;
        }
    }
}
