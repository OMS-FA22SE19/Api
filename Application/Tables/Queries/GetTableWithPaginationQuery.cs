﻿using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Tables.Queries
{
    public class GetTableWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<TableDto>>>
    {
        public TableProperty? OrderBy { get; init; }
    }

    public class GetTableWithPaginationQueryHandler : IRequestHandler<GetTableWithPaginationQuery, Response<PaginatedList<TableDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTableWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<TableDto>>> Handle(GetTableWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Table, bool>>> filters = new();
            Func<IQueryable<Table>, IOrderedQueryable<Table>> orderBy = null;
            string includeProperties = $"{nameof(Table.TableType)}";

            filters.Add(e => !e.IsDeleted);
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
                    break;
            }

            var result = await _unitOfWork.TableRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Table>, PaginatedList<TableDto>>(result);
            return new Response<PaginatedList<TableDto>>(mappedResult);
        }
    }
}
