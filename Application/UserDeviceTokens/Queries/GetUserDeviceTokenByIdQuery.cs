using Application.Common.Exceptions;
using Application.Models;
using Application.UserDeviceTokens.Responses;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.UserDeviceTokens.Queries
{
    public class GetUserDeviceTokenWithIdQuery : IRequest<Response<UserDeviceTokenDto>>
    {
        [Required]
        public string userId { get; init; }
    }

    public class GetUserDeviceTokenWithIdQueryHandler : IRequestHandler<GetUserDeviceTokenWithIdQuery, Response<UserDeviceTokenDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserDeviceTokenWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<UserDeviceTokenDto>> Handle(GetUserDeviceTokenWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.UserDeviceTokenRepository.GetAsync(e => e.userId.Equals(request.userId));
            if (result is null)
            {
                throw new NotFoundException("Can not find token for user: " + request.userId);
            }
            var mappedResult = _mapper.Map<UserDeviceTokenDto>(result);
            return new Response<UserDeviceTokenDto>(mappedResult);
        }
    }
}
