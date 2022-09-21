using Application.Categories.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Categories.Queries
{
    public sealed class GetCategoryWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<CategoryDto>>>
    {
        public new CategoryProperty? OrderBy { get; init; }
        public bool? Available { get; init; }
    }

    public sealed class GetCategoryWithPaginationQueryHandler : IRequestHandler<GetCategoryWithPaginationQuery, Response<PaginatedList<CategoryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCategoryWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<CategoryDto>>> Handle(GetCategoryWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Category, bool>>> filters = new();
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Name.Contains(request.SearchValue) || e.Id.Equals(request.SearchValue));
            }

            switch (request.OrderBy)
            {
                case (CategoryProperty.Name):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case (CategoryProperty.Id):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Id);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Id);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.CategoryRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Category>, PaginatedList<CategoryDto>>(result);
            return new Response<PaginatedList<CategoryDto>>(mappedResult);
        }
    }
}
