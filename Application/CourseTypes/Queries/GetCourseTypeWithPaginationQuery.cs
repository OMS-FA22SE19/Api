using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.CourseTypes.Queries
{
    public sealed class GetCourseTypeWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<CourseTypeDto>>>
    {
        public CourseTypeProperty? OrderBy { get; init; }
    }

    public sealed class GetCourseTypeWithPaginationQueryHandler : IRequestHandler<GetCourseTypeWithPaginationQuery, Response<PaginatedList<CourseTypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCourseTypeWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<CourseTypeDto>>> Handle(GetCourseTypeWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<CourseType, bool>>> filters = new();
            Func<IQueryable<CourseType>, IOrderedQueryable<CourseType>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Name.Contains(request.SearchValue) || request.SearchValue.Contains(e.Id.ToString()));
            }

            switch (request.OrderBy)
            {
                case (CourseTypeProperty.Name):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case (CourseTypeProperty.Id):
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

            var result = await _unitOfWork.CourseTypeRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<CourseType>, PaginatedList<CourseTypeDto>>(result);
            return new Response<PaginatedList<CourseTypeDto>>(mappedResult);
        }
    }
}
