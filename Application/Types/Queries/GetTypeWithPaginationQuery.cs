using Application.Common.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Common;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Types.Queries
{
    public sealed class GetTypeWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<TypeDto>>>
    {
        public TypeProperty? OrderBy { get; init; }
    }

    public sealed class GetTypeWithPaginationQueryHandler : IRequestHandler<GetTypeWithPaginationQuery, Response<PaginatedList<TypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTypeWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<TypeDto>>> Handle(GetTypeWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Core.Entities.Type, bool>>> filters = new();
            Func<IQueryable<Core.Entities.Type>, IOrderedQueryable<Core.Entities.Type>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Name.Contains(request.SearchValue) || request.SearchValue.Equals(e.Id.ToString()));
            }

            switch (request.OrderBy)
            {
                case (TypeProperty.Name):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case (TypeProperty.Id):
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

            var result = await _unitOfWork.TypeRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Core.Entities.Type>, PaginatedList<TypeDto>>(result);
            return new Response<PaginatedList<TypeDto>>(mappedResult);
        }
    }
}
