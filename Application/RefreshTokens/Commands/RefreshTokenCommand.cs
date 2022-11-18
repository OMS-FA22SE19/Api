using Application.Authentication.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Models;
using Application.RefreshTokens.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading;

namespace Application.RefreshTokens.Commands
{
    public class RefreshTokenCommand : IMapFrom<RefreshToken>, IRequest<Response<AuthenticationResponse>>
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string ipAddress { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RefreshTokenCommand, RefreshToken>();
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Response<AuthenticationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthenticationService _authenticationService;

        public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _authenticationService = authenticationService;
        }

        public async Task<Response<AuthenticationResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = getUserByRefreshToken(request.token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == request.token);

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                revokeDescendantRefreshTokens(refreshToken, user, request.ipAddress, $"Attempted reuse of revoked ancestor token: {request.token}");
                await _unitOfWork.UserRepository.UpdateAsync(user);
                _unitOfWork.CompleteAsync(cancellationToken);
            }

            if (!refreshToken.IsActive)
                throw new NotFoundException("Invalid token");

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = await rotateRefreshToken(refreshToken, request.ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            // remove old refresh tokens from user
            removeOldRefreshTokens(user);

            // save changes to db
            await _unitOfWork.UserRepository.UpdateAsync(user);
            _unitOfWork.CompleteAsync(cancellationToken);

            // generate new jwt
            var tokenHandler = new JwtSecurityTokenHandler();
            string rolename = ((List<string>)await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var claims = await _userManager.GetClaimsAsync(user);
            claims.Add(new Claim(ClaimTypes.Role, rolename));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            var jwtToken = _authenticationService.GenerateJwtToken(user, claims);

            var response = new AuthenticationResponse
            {
                Email = user.Email,
                JwtToken = tokenHandler.WriteToken(jwtToken),
                ExpiredAt = jwtToken.ValidTo
            };
            return new Response<AuthenticationResponse>(response)
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        //helpers
        private ApplicationUser getUserByRefreshToken(string token)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                throw new NotFoundException("Invalid token");

            return user;
        }

        private async Task<RefreshToken> rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = await _authenticationService.GenerateRefreshToken(ipAddress);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private void removeOldRefreshTokens(ApplicationUser user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            //user.RefreshTokens.RemoveAll(x =>
            //    !x.IsActive &&
            //    x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
            foreach(var token in user.RefreshTokens)
            {
                user.RefreshTokens.Remove(token);
            }
        }

        private void revokeDescendantRefreshTokens(RefreshToken refreshToken, ApplicationUser user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                if (childToken.IsActive)
                    revokeRefreshToken(childToken, ipAddress, reason);
                else
                    revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }

        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
    }
}
