using Application.Models;
using Application.AdminSettings.Response;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using Application.Common.Exceptions;
using Core.Entities;
using System.Text.Json;
using Application.Tables.Response;
using System.Text.Json.Nodes;
using Application.AdminSettings.Responses;
using System.Dynamic;

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

            var settings = await _unitOfWork.AdminSettingRepository.GetAllAsync();
            settings.Remove(settings.Single(s => s.Name.Equals("json")));

            var jsonString = @"
                {
                    ""AdminSettings"":{
                    }
                }
                ";

            var jsonDoc = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            var adminSettingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonDoc["AdminSettings"].ToString());
            foreach (var setting in settings)
            {
                adminSettingsDict.Add(setting.Name, setting.Value);
            }
            jsonDoc["AdminSettings"] = adminSettingsDict;

            var json = await _unitOfWork.AdminSettingRepository.GetAsync(a => a.Name.Equals("json"));
            json.Value = JsonSerializer.Serialize(jsonDoc);

            var jsonResult = await _unitOfWork.AdminSettingRepository.UpdateAsync(json);

            await _unitOfWork.CompleteAsync(cancellationToken);

            if (result is null)
            {
                return new Response<AdminSettingDto>("error");
            }
            if (jsonResult is null)
            {
                return new Response<AdminSettingDto>("json error");
            }

            return new Response<AdminSettingDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
