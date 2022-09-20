using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Menus.Queries
{
    public sealed class GetMenuWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<MenuDto>>>
    {
        public new MenuProperty? OrderBy { get; init; }
        public bool? IsHidden { get; init; }
    }

    public sealed class GetMenuWithPaginationQueryHandler : IRequestHandler<GetMenuWithPaginationQuery, Response<PaginatedList<MenuDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMenuWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<MenuDto>>> Handle(GetMenuWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Menu, bool>>> filters = new();
            Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Name.Contains(request.SearchValue)
                    || e.Description.Contains(request.SearchValue)
                    || request.SearchValue.Equals(e.Id));
            }
            if (request.IsHidden != null)
            {
                filters.Add(e => e.IsHidden == request.IsHidden);
            }

            switch (request.OrderBy)
            {
                case (MenuProperty.Name):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case (MenuProperty.Description):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Description);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Description);
                    break;
                case (MenuProperty.IsHidden):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.IsHidden);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.IsHidden);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.MenuRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Menu>, PaginatedList<MenuDto>>(result);
            return new Response<PaginatedList<MenuDto>>(mappedResult);
        }
    }
}
