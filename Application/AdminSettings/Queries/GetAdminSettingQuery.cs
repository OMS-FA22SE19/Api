using Application.AdminSettings.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;

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
            Func<IQueryable<AdminSetting>, IOrderedQueryable<AdminSetting>> orderBy = e => e.OrderBy(x => x.Order);
            var settings = await _unitOfWork.AdminSettingRepository.GetAllAsync(orderBy: orderBy);

            var mappedResult = _mapper.Map<List<AdminSettingDto>>(settings);
            return new Response<List<AdminSettingDto>>(mappedResult);
        }
    }
}
