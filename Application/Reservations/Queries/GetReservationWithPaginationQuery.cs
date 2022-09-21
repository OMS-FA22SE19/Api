﻿using Application.Reservations.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;
using Application.Foods.Response;

namespace Application.Reservations.Queries
{
    public class GetReservationWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<ReservationDto>>>
    {
        public string userId { get; set; }
        public ReservationStatus? Status { get; init; }
    }

    public class GetReservationWithPaginationQueryHandler : IRequestHandler<GetReservationWithPaginationQuery, Response<PaginatedList<ReservationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReservationWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<ReservationDto>>> Handle(GetReservationWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Reservation, bool>>> filters = new();
            Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.userId))
            {
                filters.Add(e => e.UserId.Contains(request.userId));
            }
            if (request.Status is not null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            orderBy = e => e.OrderByDescending(x => x.Date);

            var result = await _unitOfWork.ReservationRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Reservation>, PaginatedList<ReservationDto>>(result);
            return new Response<PaginatedList<ReservationDto>>(mappedResult);
        }
    }
}