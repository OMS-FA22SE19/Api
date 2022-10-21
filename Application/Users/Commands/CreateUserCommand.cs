using Application.Common.Mappings;
using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Firebase.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands
{
    public class CreateUserCommand : IMapFrom<ApplicationUser>, IRequest<Response<UserDto>>
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateUserCommand, ApplicationUser>();
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser() { UserName = request.Email.Split('@')[0], Email = request.Email, FullName = request.FullName, PhoneNumber = request.PhoneNumber };
            if (await _userManager.FindByNameAsync(request.PhoneNumber) is not null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userRole = await _roleManager.FindByNameAsync(request.Role);
            if (userRole is null)
            {
                throw new ArgumentNullException("Not found role: " + request.Role);
            }

            var password = request.Password;
            var result = await _userManager.CreateAsync(user, password);
            await _userManager.AddToRoleAsync(user, userRole.Name);
            var mappedResult = _mapper.Map<UserDto>(user);
            return new Response<UserDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
