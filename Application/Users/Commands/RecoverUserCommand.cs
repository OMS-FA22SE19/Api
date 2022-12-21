using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Commands
{
    public sealed class RecoverUserCommand : IRequest<Response<UserDto>>
    {
        [Required]
        public string Id { get; init; }
    }

    public sealed class RecoverUserCommandHandler : IRequestHandler<RecoverUserCommand, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RecoverUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<UserDto>> Handle(RecoverUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id.Equals(request.Id) && u.IsDeleted);
            if (user is null)
            {
                throw new NullReferenceException("Not found user");
            }
            user.IsDeleted = false;
            var result = await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<UserDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

