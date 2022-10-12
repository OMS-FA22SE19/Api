using Application.Common.Exceptions;
using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
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
            var result = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Reservation.ReservationTables)}.{nameof(ReservationTable.Table)}.{nameof(Table.TableType)},{nameof(Reservation.User)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Reservation), $"with {request.Id}");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);
            return new Response<ReservationDto>(mappedResult);
        }
    }
}
