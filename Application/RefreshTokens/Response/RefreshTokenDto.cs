using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.RefreshTokens.Response
{
    public class RefreshTokenDto : IMapFrom<RefreshToken>
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public string ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();
        }
    }
}
