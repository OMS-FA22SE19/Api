using Application.Common.Exceptions;
using Application.Common.Models;
using Application.UserDeviceTokens.Responses;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.UserDeviceTokens.Queries
{
    public class GetAllUserDeviceTokenQuery : IRequest<Response<List<UserDeviceTokenDto>>>
    {
    }

    public class GetAllUserDeviceTokenQueryHandler : IRequestHandler<GetAllUserDeviceTokenQuery, Response<List<UserDeviceTokenDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllUserDeviceTokenQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<UserDeviceTokenDto>>> Handle(GetAllUserDeviceTokenQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.UserDeviceTokenRepository.GetAllAsync();
            var mappedResult = _mapper.Map<List<UserDeviceTokenDto>>(result);
            return new Response<List<UserDeviceTokenDto>>(mappedResult);
        }
    }
}
