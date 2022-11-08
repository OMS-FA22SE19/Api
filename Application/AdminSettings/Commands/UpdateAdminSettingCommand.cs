using Application.AdminSettings.Response;
using Application.Common.Exceptions;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.AdminSettings.Commands
{
    public sealed class UpdateAdminSettingCommand : IRequest<Response<AdminSettingDto>>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }
    }

    public sealed class UpdateAdminSettingCommandHandler : IRequestHandler<UpdateAdminSettingCommand, Response<AdminSettingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateAdminSettingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<AdminSettingDto>> Handle(UpdateAdminSettingCommand request, CancellationToken cancellationToken)
        {
            var updateSetting = await _unitOfWork.AdminSettingRepository.GetAsync(a => a.Name.Equals(request.Name));
            if (updateSetting is null)
            {
                throw new NotFoundException(nameof(AdminSetting), request.Name);
            }

            updateSetting.Value = request.Value;

            var result = await _unitOfWork.AdminSettingRepository.UpdateAsync(updateSetting);

            await _unitOfWork.CompleteAsync(cancellationToken);

            if (result is null)
            {
                return new Response<AdminSettingDto>("error");
            }

            return new Response<AdminSettingDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
