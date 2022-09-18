using Core.Entities;

namespace Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser appUser);
    }
}
