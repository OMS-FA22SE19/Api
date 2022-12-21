using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands
{
    public sealed class DeleteUserCommand : IRequest<Response<UserDto>>
    {
        [Required]
        public string Id { get; init; }
    }

    public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Response<UserDto>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id.Equals(request.Id));
            if (user is null)
            {
                throw new NullReferenceException("Not found user");
            }
            if (user.Id.Equals(_currentUserService?.UserId))
            {
                throw new BadRequestException("You cannot disable yourself");
            }
            var result = await _unitOfWork.UserRepository.DeleteAsync(user);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<UserDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

