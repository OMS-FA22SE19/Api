using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Users.Queries
{
    public sealed class GetUserWithIdQuery : IRequest<Response<UserDto>>
    {
        [Required]
        public string Id { get; init; }
    }

    public sealed class GetUserWithIdQueryHandler : IRequestHandler<GetUserWithIdQuery, Response<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<UserDto>> Handle(GetUserWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.UserRepository.GetAsync(e => e.Id == request.Id);
            if (result is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.Id);
            }
            var mappedResult = _mapper.Map<UserDto>(result);
            return new Response<UserDto>(mappedResult);
        }
    }
}
