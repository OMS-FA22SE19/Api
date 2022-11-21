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
using FirebaseAdmin.Auth.Hash;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;

namespace Application.RefreshTokens.Commands
{
    public class AuthenticationCommand : IMapFrom<RefreshToken>, IRequest<Response<AuthenticationResponse>>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [JsonIgnore]
        public string? ipAddress { get; set; }
    }

    public class AuthenticationCommandHandler : IRequestHandler<AuthenticationCommand, Response<AuthenticationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _authenticationService = authenticationService;
        }

        public async Task<Response<AuthenticationResponse>> Handle(AuthenticationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null)
                {
                    throw new NotFoundException("Not found account");
                }
                if (!(await _userManager.CheckPasswordAsync(user, request.Password)))
                {
                    throw new BadRequestException("Not valid login information");
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
                var token = _authenticationService.GenerateJwtToken(user, claims);

                // remove old refresh tokens from user
                List<Expression<Func<RefreshToken, bool>>> filters = new();
                filters.Add(rt => rt.UserId.Equals(user.Id));
                var oldTokens = await _unitOfWork.RefreshTokenRepository.GetAllAsync(filters);
                user.RefreshTokens = oldTokens;
                await removeOldRefreshTokensAsync(user);

                var refreshToken = await _authenticationService.GenerateRefreshToken(request.ipAddress);
                //user.RefreshTokens.Add(refreshToken);

                refreshToken.UserId = user.Id;
                // save changes to db
                //await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.RefreshTokenRepository.InsertAsync(refreshToken);
                await _unitOfWork.CompleteAsync(cancellationToken);

                var response = new AuthenticationResponse
                {
                    Email = user.Email,
                    JwtToken = tokenHandler.WriteToken(token),
                    ExpiredAt = token.ValidTo,
                    RefreshToken = refreshToken.Token
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

        //helpers
        private async Task removeOldRefreshTokensAsync(ApplicationUser user)
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
    }
}
