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

        //public async Task<AuthenticationResponse> RefreshToken(string token, string ipAddress)
        //{
        //    var user = getUserByRefreshToken(token);
        //    var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        //    if (refreshToken.IsRevoked)
        //    {
        //        // revoke all descendant tokens in case this token has been compromised
        //        revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
        //        await _unitOfWork.UserRepository.UpdateAsync(user);
        //        _unitOfWork.CompleteAsync(cancellationToken);
        //    }

        //    if (!refreshToken.IsActive)
        //        throw new NotFoundException("Invalid token");

        //    // replace old refresh token with a new one (rotate token)
        //    var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
        //    user.RefreshTokens.Add(newRefreshToken);

        //    // remove old refresh tokens from user
        //    removeOldRefreshTokens(user);

        //    // save changes to db
        //    await _unitOfWork.UserRepository.UpdateAsync(user);
        //    _unitOfWork.CompleteAsync(cancellationToken);

        //    // generate new jwt
        //    var jwtToken = _jwtUtils.GenerateJwtToken(user);

        //    return new AuthenticationResponse(user, jwtToken, newRefreshToken.Token);
        //}

        //public async Task RevokeTokenAsync(string token, string ipAddress)
        //{
        //    var user = getUserByRefreshToken(token);
        //    var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        //    if (!refreshToken.IsActive)
        //        throw new NotFoundException("Invalid token");

        //    // revoke token and save
        //    revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        //    await _unitOfWork.UserRepository.UpdateAsync(user);
        //    _unitOfWork.CompleteAsync(cancellationToken);
        //}

        ////helpers
        //private ApplicationUser getUserByRefreshToken(string token)
        //{
        //    var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

        //    if (user == null)
        //        throw new NotFoundException("Invalid token");

        //    return user;
        //}

        //private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        //{
        //    var newRefreshToken = GenerateRefreshToken(ipAddress);
        //    revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        //    return newRefreshToken;
        //}

        //private void removeOldRefreshTokens(ApplicationUser user)
        //{
        //    // remove old inactive refresh tokens from user based on TTL in app settings
        //    user.RefreshTokens.RemoveAll(x =>
        //        !x.IsActive &&
        //        x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        //}

        //private void revokeDescendantRefreshTokens(RefreshToken refreshToken, ApplicationUser user, string ipAddress, string reason)
        //{
        //    // recursively traverse the refresh token chain and ensure all descendants are revoked
        //    if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        //    {
        //        var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        //        if (childToken.IsActive)
        //            revokeRefreshToken(childToken, ipAddress, reason);
        //        else
        //            revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
        //    }
        //}

        //private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        //{
        //    token.Revoked = DateTime.UtcNow;
        //    token.RevokedByIp = ipAddress;
        //    token.ReasonRevoked = reason;
        //    token.ReplacedByToken = replacedByToken;
        //}

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
                var users = await _unitOfWork.UserRepository.GetAllAsync();
                var tokenIsUnique = users.Any(u => u.RefreshTokens.Any(rf => rf.Token.Equals(token)));

                if (!tokenIsUnique)
                    return await getUniqueToken();

                return token;
            }
        }
    }
}
