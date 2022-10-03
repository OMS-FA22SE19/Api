using Application.Common.Exceptions;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;

namespace Application.Menus.Queries
{
    public sealed class GetAvailableMenuQuery : IRequest<Response<MenuDto>>
    {
    }

    public sealed class GetAvailableMenuQueryHandler : IRequestHandler<GetAvailableMenuQuery, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAvailableMenuQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(GetAvailableMenuQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.MenuRepository.GetAsync(e => !e.IsHidden);
            if (result is null)
            {
                throw new NotFoundException($"No available {nameof(Menu)}");
            }
            var mappedResult = _mapper.Map<MenuDto>(result);
            return new Response<MenuDto>(mappedResult);
        }
    }
}
