﻿using Application.Reservations.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Queries
{
    public class GetReservationWithIdQuery : IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public class GetReservationWithIdQueryHandler : IRequestHandler<GetReservationWithIdQuery, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReservationWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDto>> Handle(GetReservationWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            var mappedResult = _mapper.Map<ReservationDto>(result);
            return new Response<ReservationDto>(mappedResult);
        }
    }
}