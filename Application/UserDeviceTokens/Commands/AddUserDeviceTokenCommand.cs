using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.UserDeviceTokens.Responses;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.UserDeviceTokens.Commands
{
    public class AddUserDeviceTokenCommand : IMapFrom<UserDeviceToken>, IRequest<Response<UserDeviceTokenDto>>
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string deviceToken { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AddUserDeviceTokenCommand, UserDeviceToken>();
        }
    }

    public class CreateUserDeviceTokenCommandHandler : IRequestHandler<AddUserDeviceTokenCommand, Response<UserDeviceTokenDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateUserDeviceTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<UserDeviceTokenDto>> Handle(AddUserDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(e => e.Id == request.userId);
            if (user is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.userId);
            }
            UserDeviceToken result = null;
            var userDeviceToken = await _unitOfWork.UserDeviceTokenRepository.GetAsync(d => d.userId.Equals(request.userId));
            if (userDeviceToken is not null)
            {
                userDeviceToken.deviceToken = request.deviceToken;
                result = await _unitOfWork.UserDeviceTokenRepository.UpdateAsync(userDeviceToken);

            }
            else
            {
                var entity = _mapper.Map<UserDeviceToken>(request);

                result = await _unitOfWork.UserDeviceTokenRepository.InsertAsync(entity);
            }
            
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<UserDeviceTokenDto>("error");
            }
            var mappedResult = _mapper.Map<UserDeviceTokenDto>(result);
            return new Response<UserDeviceTokenDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
