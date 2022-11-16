using Application.Authentication.Models;
using Application.Common.Interfaces;
using Application.Helpers;
using Application.Models;
using Core.Entities;
using Firebase.Auth;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppSettings _appSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationService(IOptions<AppSettings> appSettings, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
        }

        public async Task<Response<AuthenticationResponse>> Authenticate(AuthenticationRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    throw new NullReferenceException("Not found account");
                }
                if (!(await _userManager.CheckPasswordAsync(user, request.Password)))
                {
                    throw new NullReferenceException("Not valid login information");
                }
                if (user.IsDeleted == true)
                {
                    throw new NullReferenceException("This user has been deleted");
                }


                var tokenHandler = new JwtSecurityTokenHandler();
                // TODO: Remove this before demo
                string rolename = ((List<string>)await _userManager.GetRolesAsync(user)).FirstOrDefault();

                var claims = await _userManager.GetClaimsAsync(user);
                claims.Add(new Claim(ClaimTypes.Role, rolename));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                var token = GenerateJwtToken(user, claims);
                var response = new AuthenticationResponse
                {
                    Email = user.Email,
                    JwtToken = tokenHandler.WriteToken(token),
                    ExpiredAt = token.ValidTo
                };
                return new Response<AuthenticationResponse>(response)
                {
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (NullReferenceException ex)
            {
                return new Response<AuthenticationResponse>(ex.Message)
                {
                    StatusCode = HttpStatusCode.NotFound,
                };
            }
        }


        private SecurityToken GenerateJwtToken(ApplicationUser user, IList<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var claimsList = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                claimsList.Add(claim.Type, claim.Value);
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Claims = claimsList,
                Issuer = _appSettings.ValidIssuer,
                Audience = _appSettings.ValidAudience,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpireHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
