using Application.Reservations.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public sealed class DeleteReservationCommand : IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDto>> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.ReservationRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<ReservationDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

