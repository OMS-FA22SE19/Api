using Application.AdminSettings.Response;
using Application.Models;
using AutoMapper;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Core.Interfaces;
using Application.AdminSettings.Responses;

namespace Application.AdminSettings.Queries
{
    public sealed class GetAdminSettingQuery : IRequest<Response<ListOfAdminSettingDto>>
    {
    }

    public sealed class GetAdminSettingQueryHandler : IRequestHandler<GetAdminSettingQuery, Response<ListOfAdminSettingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAdminSettingQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ListOfAdminSettingDto>> Handle(GetAdminSettingQuery request, CancellationToken cancellationToken)
        {
            var settings = await _unitOfWork.AdminSettingRepository.GetAllAsync();
            settings.Remove(settings.Single(s => s.Name.Equals("json")));

            ListOfAdminSettingDto adminSettings = new ListOfAdminSettingDto() 
            { 
                adminSettings = new List<AdminSettingDto>()
            };
            foreach(var setting in settings)
            {
                var mappedSetting = _mapper.Map<AdminSettingDto>(setting);
                adminSettings.adminSettings.Add(mappedSetting);
            }
            return new Response<ListOfAdminSettingDto>(adminSettings);
        }
    }
}
