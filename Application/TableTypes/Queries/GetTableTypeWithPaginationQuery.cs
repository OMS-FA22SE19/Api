using Application.Models;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.TableTypes.Queries
{
    public sealed class GetTableTypeWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<TableTypeDto>>>
    {
        public TableTypeProperty? OrderBy { get; set; }
    }

    public sealed class GetTableTypeWithPaginationQueryHandler : IRequestHandler<GetTableTypeWithPaginationQuery, Response<PaginatedList<TableTypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTableTypeWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<TableTypeDto>>> Handle(GetTableTypeWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<TableType, bool>>> filters = new();
            Func<IQueryable<TableType>, IOrderedQueryable<TableType>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                if (double.TryParse(request.SearchValue, out double chargePerSeat))
                {
                    filters.Add(e => e.ChargePerSeat == chargePerSeat);
                }
                else
                {
                    filters.Add(e => e.Name.Contains(request.SearchValue));
                }
            }

            switch (request.OrderBy)
            {
                case TableTypeProperty.Id:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Id);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Id);
                    break;
                case TableTypeProperty.Name:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case TableTypeProperty.ChargePerSeat:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.ChargePerSeat);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.ChargePerSeat);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.TableTypeRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);

            var mappedResult = _mapper.Map<PaginatedList<TableType>, PaginatedList<TableTypeDto>>(result);
            foreach (var item in mappedResult)
            {
                var tableFilters = new List<Expression<Func<Table, bool>>>();
                tableFilters.Add(e => e.TableTypeId == item.Id && !e.IsDeleted);
                var tablesInType = await _unitOfWork.TableRepository.GetAllAsync(tableFilters);
                item.Quantity = tablesInType.Count;
            }
            return new Response<PaginatedList<TableTypeDto>>(mappedResult);
        }
    }
}
