using Application.Common.Exceptions;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Queries
{
    public sealed class GetMenuWithIdQuery : IRequest<Response<MenuDto>>
    {
        [Required]
        public int Id { get; init; }
        public bool? IsHidden { get; set; }
    }

    public sealed class GetMenuWithIdQueryHandler : IRequestHandler<GetMenuWithIdQuery, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMenuWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(GetMenuWithIdQuery request, CancellationToken cancellationToken)
        {
            Menu result = new();
            if (request.IsHidden != null)
            {
                result = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id && e.IsHidden == request.IsHidden);
            }
            else
            {
                result = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id);
            }
            if (result is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }
            var mappedResult = _mapper.Map<MenuDto>(result);
            return new Response<MenuDto>(mappedResult);
        }
    }
}
