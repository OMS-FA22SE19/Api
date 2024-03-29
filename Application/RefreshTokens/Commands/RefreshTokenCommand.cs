﻿using Application.Authentication.Models;
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
using System.Linq.Expressions;
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
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetAsync(t => t.UserId.Equals(user.Id) && t.Token.Equals(request.token));

            if (refreshToken.IsRevoked)
            {
                revokeDescendantRefreshTokens(refreshToken, user, request.ipAddress, $"Attempted reuse of revoked ancestor token: {request.token}");
                throw new UnauthorizedAccessException("token Revoked");
            }

            if (!refreshToken.IsActive)
                throw new NotFoundException("Token is not active");

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = await rotateRefreshToken(refreshToken, request.ipAddress);
            newRefreshToken.UserId = user.Id;
            await _unitOfWork.RefreshTokenRepository.InsertAsync(newRefreshToken);


            // remove old refresh tokens from user
            List<Expression<Func<RefreshToken, bool>>> filters = new();
            filters.Add(rt => rt.UserId.Equals(user.Id));
            var oldTokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(filters);
            user.RefreshTokens = oldTokens;
            await removeOldRefreshTokens(user);

            // save changes to db
            await _unitOfWork.CompleteAsync(cancellationToken);

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
                ExpiredAt = jwtToken.ValidTo,
                RefreshToken = newRefreshToken.Token
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
            await _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);
            return newRefreshToken;
        }

        private async Task removeOldRefreshTokens(ApplicationUser user)
        {
            if (user.RefreshTokens is not null)
            {
                foreach (var token in user.RefreshTokens.ToList())
                {
                    if (!token.IsActive && token.Created.AddDays(2) <= DateTime.UtcNow)
                    {
                        await _unitOfWork.RefreshTokenRepository.DeleteAsync(t => t.Token.Equals(token.Token));
                        await _unitOfWork.CompleteAsync(default);
                    }
                }
            }
        }

        private async void revokeDescendantRefreshTokens(RefreshToken refreshToken, ApplicationUser user, string ipAddress, string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = await _unitOfWork.RefreshTokenRepository.GetAsync(ct => ct.Token == refreshToken.ReplacedByToken);
                if (childToken.IsActive)
                {
                    revokeRefreshToken(childToken, ipAddress, reason);
                    await _unitOfWork.RefreshTokenRepository.UpdateAsync(childToken);
                    await _unitOfWork.CompleteAsync(default);
                }
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
