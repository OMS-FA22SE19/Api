using Application.Models;
using Application.Users.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Users.Queries
{
    public sealed class GetUserWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<UserDto>>>
    {
        public UserProperty SearchBy { get; set; }
        public UserProperty? OrderBy { get; set; }
    }

    public sealed class GetUserWithPaginationQueryHandler : IRequestHandler<GetUserWithPaginationQuery, Response<PaginatedList<UserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<UserDto>>> Handle(GetUserWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<ApplicationUser, bool>>> filters = new();
            Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case UserProperty.Id:
                        filters.Add(e => e.Id.Contains(request.SearchValue));
                        break;
                    case UserProperty.UserName:
                        filters.Add(e => e.UserName.Contains(request.SearchValue));
                        break;
                    case UserProperty.FullName:
                        filters.Add(e => e.FullName.Contains(request.SearchValue));
                        break;
                    case UserProperty.Email:
                        filters.Add(e => e.Email.Contains(request.SearchValue));
                        break;
                    case UserProperty.PhoneNumber:
                        filters.Add(e => e.PhoneNumber.Contains(request.SearchValue));
                        break;
                    default:
                        break;
                }
            }

            switch (request.OrderBy)
            {
                case UserProperty.Id:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Id);
                    }
                    orderBy = e => e.OrderBy(x => x.Id);
                    break;
                case UserProperty.FullName:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.FullName);
                    }
                    orderBy = e => e.OrderBy(x => x.FullName);
                    break;
                case UserProperty.PhoneNumber:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.PhoneNumber);
                    }
                    orderBy = e => e.OrderBy(x => x.PhoneNumber);
                    break;
                case UserProperty.UserName:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.UserName);
                    }
                    orderBy = e => e.OrderBy(x => x.UserName);
                    break;
                case UserProperty.Email:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Email);
                    }
                    orderBy = e => e.OrderBy(x => x.Email);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.UserRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<ApplicationUser>, PaginatedList<UserDto>>(result);
            return new Response<PaginatedList<UserDto>>(mappedResult);
        }
    }
}
