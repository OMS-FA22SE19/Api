using Application.Common.Interfaces;
using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Tables.Queries
{
    public class GetStatusOfTablesQuery : PaginationRequest, IRequest<Response<List<TableStatusDto>>>
    {
        public TableProperty? OrderBy { get; init; }
        public class GetStatusOfTableQueryHandler : IRequestHandler<GetStatusOfTablesQuery, Response<List<TableStatusDto>>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IDateTime _dateTime;

            public GetStatusOfTableQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTime dateTime)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _dateTime = dateTime;
            }

            public async Task<Response<List<TableStatusDto>>> Handle(GetStatusOfTablesQuery request, CancellationToken cancellationToken)
            {
                List<Expression<Func<Table, bool>>> filters = new();
                Func<IQueryable<Table>, IOrderedQueryable<Table>> orderBy = null;
                string includeProperties = $"{nameof(Table.TableType)},{nameof(Table.Orders)},{nameof(Table.ReservationsTables)}.{nameof(ReservationTable.Reservation)}";

                switch (request.OrderBy)
                {
                    case (TableProperty.NumOfSeats):
                        if (request.IsDescending)
                        {
                            orderBy = e => e.OrderByDescending(x => x.NumOfSeats);
                            break;
                        }
                        orderBy = e => e.OrderBy(x => x.NumOfSeats);
                        break;
                    case (TableProperty.Status):
                        if (request.IsDescending)
                        {
                            orderBy = e => e.OrderByDescending(x => x.Status);
                            break;
                        }
                        orderBy = e => e.OrderBy(x => x.Status);
                        break;
                    case (TableProperty.Type):
                        if (request.IsDescending)
                        {
                            orderBy = e => e.OrderByDescending(x => x.TableType.Name);
                            break;
                        }
                        orderBy = e => e.OrderBy(x => x.TableType.Name);
                        break;
                    default:
                        orderBy = e => e.OrderBy(x => x.Id);
                        break;
                }
                var result = await _unitOfWork.TableRepository.GetAllAsync(filters, orderBy, includeProperties);

                var mappedResult = _mapper.Map<List<Table>, List<TableStatusDto>>(result);
                foreach (var item in mappedResult)
                {
                    string userId = null;
                    string fullName = null;
                    bool isChanged = false;
                    var table = result.FirstOrDefault(e => e.Id == item.Id);

                    var tableReservation =  table.ReservationsTables.ToList();
                    var reservationTable = tableReservation.FirstOrDefault(rt => rt.Reservation.StartTime <= _dateTime.Now && rt.Reservation.EndTime > _dateTime.Now);
                    if (reservationTable is not null)
                    {
                        userId = reservationTable.Reservation.UserId;
                        fullName = reservationTable.Reservation.User?.FullName;
                        isChanged = table.Status != TableStatus.Reserved;
                        table.Status = TableStatus.Reserved;
                    }

                    var order = table.Orders.FirstOrDefault(e => e.Date <= _dateTime.Now && e.Status == OrderStatus.Processing);
                    if (order is not null)
                    {
                        item.OrderId = order.Id;
                        userId = reservationTable.Reservation.UserId;
                        fullName = reservationTable.Reservation.User?.FullName;
                        isChanged = table.Status != TableStatus.Occupied;
                        table.Status = TableStatus.Occupied;
                    }

                    item.Status = table.Status;
                    item.UserId = userId;
                    item.FullName = fullName;
                    if (isChanged)
                    {
                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }
                }
                await _unitOfWork.CompleteAsync(cancellationToken);
                return new Response<List<TableStatusDto>>(mappedResult);
            }
        }
    }
}
