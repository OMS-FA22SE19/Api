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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading;

namespace Application.RefreshTokens.Commands
{
    public class RevokeTokenCommand : IMapFrom<RefreshToken>, IRequest<Response<AuthenticationResponse>>
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string ipAddress { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RevokeTokenCommand, RefreshToken>();
        }
    }

    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Response<AuthenticationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthenticationService _authenticationService;

        public RevokeTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _authenticationService = authenticationService;
        }

        public async Task<Response<AuthenticationResponse>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            var user = getUserByRefreshToken(request.token);
            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetAsync(t => t.UserId.Equals(user.Id) && t.Token.Equals(request.token));

            if (!refreshToken.IsActive)
                throw new NotFoundException("Invalid token");

            // revoke token and save
            revokeRefreshToken(refreshToken, request.ipAddress, "Revoked without replacement");
            await _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var response = new AuthenticationResponse
            {
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

        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
    }
}
