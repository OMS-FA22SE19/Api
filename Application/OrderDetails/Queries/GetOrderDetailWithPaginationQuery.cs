using Application.Common.Interfaces;
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
        public OrderDetailProperty SearchBy { get; set; }
        public OrderDetailStatus? Status { get; set; }
        public OrderDetailProperty? OrderBy { get; set; }
    }

    public sealed class GetOrderDetailWithPaginationQueryHandler : IRequestHandler<GetOrderDetailWithPaginationQuery, Response<PaginatedList<DishDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public GetOrderDetailWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<PaginatedList<DishDto>>> Handle(GetOrderDetailWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<OrderDetail, bool>>> filters = new();
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null;
            string includeProperties = $"{nameof(OrderDetail.Order)}.{nameof(Order.User)},{nameof(OrderDetail.Order)}.{nameof(Order.Reservation)}.{nameof(Reservation.ReservationTables)},{nameof(OrderDetail.Food)}";

            //filters.Add(e => e.Order.Date <= _dateTime.Now.AddHours(-3));

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case OrderDetailProperty.TableId:
                        if (int.TryParse(request.SearchValue, out int tableId))
                        {
                            filters.Add(e => e.Order.Reservation.ReservationTables.OrderBy(x => x.TableId).First().TableId == tableId);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    case OrderDetailProperty.PhoneNumber:
                        filters.Add(e => e.Order.User.PhoneNumber.Contains(request.SearchValue));
                        break;
                    case OrderDetailProperty.OrderId:
                        filters.Add(e => e.Order.Id.Contains(request.SearchValue));
                        break;
                    case OrderDetailProperty.FoodId:
                        if (int.TryParse(request.SearchValue, out int foodId))
                        {
                            filters.Add(e => foodId == e.Food.Id);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    case OrderDetailProperty.FoodName:
                        filters.Add(e => e.Food.Name.Contains(request.SearchValue));
                        break;
                    case OrderDetailProperty.Price:
                        if (double.TryParse(request.SearchValue, out double price))
                        {
                            filters.Add(e => price == e.Price);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (request.Status is not null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            filters.Add(e => !e.IsDeleted);

            switch (request.OrderBy)
            {
                case OrderDetailProperty.OrderId:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.OrderId);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.OrderId);
                    break;
                case OrderDetailProperty.FoodId:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.FoodId);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.FoodId);
                    break;
                case OrderDetailProperty.Price:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Price);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Price);
                    break;
                case OrderDetailProperty.Status:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Status);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Status);
                    break;
                default:
                    orderBy = e => e.OrderBy(x => x.Status).ThenBy(e => e.Created);
                    break;
            }

            var mappedResult = new PaginatedList<DishDto>();
            var result = await _unitOfWork.OrderDetailRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            foreach (var orderDetail in result)
            {
                var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => orderDetail.Order.ReservationId == e.Id, $"{nameof(Reservation.ReservationTables)}");
                var mappedEntity = _mapper.Map<DishDto>(orderDetail);
                if (reservation.ReservationTables.Any())
                {
                    mappedEntity.TableId = reservation.ReservationTables.OrderBy(e => e.TableId).First().TableId;
                }
                mappedResult.Add(mappedEntity);
            }
            return new Response<PaginatedList<DishDto>>(mappedResult);
        }
    }
}
