using Application.AdminSettings.Response;
using Application.Models;
using AutoMapper;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Core.Interfaces;
using Application.AdminSettings.Responses;
using Application.CourseTypes.Response;
using Core.Common;

namespace Application.AdminSettings.Queries
{
    public sealed class GetAdminSettingQuery : IRequest<Response<List<AdminSettingDto>>>
    {
    }

    public sealed class GetAdminSettingQueryHandler : IRequestHandler<GetAdminSettingQuery, Response<List<AdminSettingDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAdminSettingQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<AdminSettingDto>>> Handle(GetAdminSettingQuery request, CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.AdminSettingRepository.GetAllAsync();

            List<AdminSettingDto> list = new List<AdminSettingDto>();
            foreach(var setting in settings)
            {
                var mappedSetting = _mapper.Map<AdminSettingDto>(setting);
                list.Add(mappedSetting);
            }
            return new Response<List<AdminSettingDto>>(list);
        }
    }
}
