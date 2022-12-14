using Application.Common.Exceptions;
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

namespace Application.Users.Commands
{
    public class RegisterUserCommand : IMapFrom<ApplicationUser>, IRequest<Response<UserDto>>
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<RegisterUserCommand, ApplicationUser>();
        }
    }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser() { UserName = request.Email.Split('@')[0], Email = request.Email, FullName = request.FullName, PhoneNumber = request.PhoneNumber };
            if (await _userManager.FindByNameAsync(request.Email.Split('@')[0]) is not null
                || await _userManager.Users.FirstOrDefaultAsync(e => e.PhoneNumber.Equals(request.PhoneNumber)) is not null)
            {
                throw new BadRequestException("Phonenumber or Email has already existed!");
            }

            var userRole = await _roleManager.FindByNameAsync("Customer");

            var password = request.Password;
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var result = await _userManager.CreateAsync(user, password);
            await _userManager.AddToRoleAsync(user, userRole.Name);
            var mappedResult = _mapper.Map<UserDto>(user);
            return new Response<UserDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                Message = code
            };
        }
    }
}
