using Application.Common.Models;
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

        public DeleteUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<UserDto>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id.Equals(request.Id));
            if (user is null)
            {
                throw new NullReferenceException("Not found user");
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

