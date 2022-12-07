using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Users.Commands
{
    public class ConfirmEmailCommand : IMapFrom<ApplicationUser>, IRequest<Response<UserDto>>
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string code { get; set; }
    }

    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Response<UserDto>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.email);
            if (user == null)
            {
                throw new NotFoundException("Can not find user");
            }
            request.code = request.code.Replace(" ", "+");
            var code = Encoding.UTF8.GetString(Convert.FromBase64String(request.code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result is not null)
            {
                user.EmailConfirmed = true;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            var mappedResult = _mapper.Map<UserDto>(user);
            return new Response<UserDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
