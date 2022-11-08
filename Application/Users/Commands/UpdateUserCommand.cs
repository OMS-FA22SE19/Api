using Application.Common.Mappings;
using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands
{
    public sealed class UpdateUserCommand : IMapFrom<ApplicationUser>, IRequest<Response<UserDto>>
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Role { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateUserCommand, ApplicationUser>();
        }
    }

    public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id.Equals(request.Id));
            if (user is null)
            {
                throw new NullReferenceException("Not found user");
            }

            var role = await _userManager.GetRolesAsync(user);

            MapToEntity(request, user);
            var result = await _unitOfWork.UserRepository.UpdateAsync(user);

            await _userManager.RemoveFromRoleAsync(user, role[0]);
            await _userManager.AddToRoleAsync(user, request.Role);
            if (result is null)
            {
                return new Response<UserDto>("error");
            }
            var mappedResult = _mapper.Map<UserDto>(user);

            return new Response<UserDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private void MapToEntity(UpdateUserCommand request, ApplicationUser? entity)
        {
            entity.FullName = request.FullName;
            entity.PhoneNumber = request.PhoneNumber;
        }
    }
}
