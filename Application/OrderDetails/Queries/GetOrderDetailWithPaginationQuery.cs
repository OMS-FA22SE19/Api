using Application.Models;
using Application.OrderDetails.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.OrderDetails.Queries
{
    public sealed class GetOrderDetailWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<DishDto>>>
    {
        public OrderDetailProperty OrderBy { get; set; }
    }

    public sealed class GetOrderDetailWithPaginationQueryHandler : IRequestHandler<GetOrderDetailWithPaginationQuery, Response<PaginatedList<DishDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderDetailWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<DishDto>>> Handle(GetOrderDetailWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<OrderDetail, bool>>> filters = new();
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null;
            string includeProperties = $"{nameof(OrderDetail.Order)},{nameof(OrderDetail.Food)}";

            filters.Add(e => e.Order.Date <= DateTime.UtcNow.AddHours(7).AddHours(-3));

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.OrderId.Contains(request.SearchValue)
                    || request.SearchValue.Equals(e.FoodId.ToString())
                    || request.SearchValue.Equals(e.Id.ToString()));
            }

            switch (request.OrderBy)
            {
                case OrderDetailProperty.OrderId:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.OrderId);
                    }
                    orderBy = e => e.OrderBy(x => x.OrderId);
                    break;
                case OrderDetailProperty.FoodId:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.FoodId);
                    }
                    orderBy = e => e.OrderBy(x => x.FoodId);
                    break;
                case OrderDetailProperty.Price:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Price);
                    }
                    orderBy = e => e.OrderBy(x => x.Price);
                    break;
                case OrderDetailProperty.Status:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Status);
                    }
                    orderBy = e => e.OrderBy(x => x.Status);
                    break;
                default:
                    orderBy = e => e.OrderByDescending(x => x.Created);
                    break;
            }

            var result = await _unitOfWork.OrderDetailRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<OrderDetail>, PaginatedList<DishDto>>(result);
            return new Response<PaginatedList<DishDto>>(mappedResult);
        }
    }
}
