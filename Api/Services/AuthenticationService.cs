using Application.Authentication.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Helpers;
using Application.Models;
using Core.Entities;
using Core.Interfaces;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppSettings _appSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(IOptions<AppSettings> appSettings, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        //method
        public async Task<Response<AuthenticationResponse>> Authenticate(AuthenticationRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(e => e.PhoneNumber.Equals(request.Email));
                    if (user is null)
                    {
                        throw new NotFoundException("Not found account");
                    }
                }
                if (user.EmailConfirmed == false) {
                    throw new BadRequestException("Please confirm your email");
                }
                if (!(await _userManager.CheckPasswordAsync(user, request.Password)))
                {
                    throw new NotFoundException("Not valid login information");
                }
                if (user.IsDeleted == true)
                {
                    throw new BadRequestException("This user has been deleted");
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

        //utils
        public SecurityToken GenerateJwtToken(ApplicationUser user, IList<Claim> claims)
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

        public int? ValidateJwtToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // return user id from JWT token if validation successful
                return userId;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }

        public async Task<RefreshToken> GenerateRefreshToken(string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Token = await getUniqueToken(),
                // token is valid for 7 days
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            return refreshToken;

            async Task<string> getUniqueToken()
            {
                // token is a cryptographically strong random sequence of values
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                // ensure token is unique by checking against db
                var users = await _unitOfWork.UserRepository.GetAllAsync(null, null, $"{nameof(ApplicationUser.RefreshTokens)}");
                var tokenIsUnique = !users.Any(u => u.RefreshTokens.Any(rf => rf.Token.Equals(token)));

                if (!tokenIsUnique)
                    return await getUniqueToken();

                return token;
            }
        }
    }
}
