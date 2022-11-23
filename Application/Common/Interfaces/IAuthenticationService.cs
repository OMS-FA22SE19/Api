using Application.Authentication.Models;
using Application.Models;
using Core.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Response<AuthenticationResponse>> Authenticate(AuthenticationRequest request);
        int? ValidateJwtToken(string token);
        Task<RefreshToken> GenerateRefreshToken(string ipAddress);
        SecurityToken GenerateJwtToken(ApplicationUser user, IList<Claim> claims);
    }
}
