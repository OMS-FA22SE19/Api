using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public sealed class DeleteReservationCommand : IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; init; }
        [Required]
        public string ReasonForCancel { get; set; }
    }

    public sealed class DeleteReservationCommandHandler : IRequestHandler<DeleteReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public DeleteReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Response<ReservationDto>> Handle(DeleteReservationCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Reservation), request.Id);
            }

            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    if (!_currentUserService.UserId.Equals(entity.UserId))
                    {
                        throw new BadRequestException("This is not your reservation");
                    }
                }
            }

            var updatedEntity = _mapper.Map<Reservation>(entity);
            updatedEntity.Status = ReservationStatus.Cancelled;
            updatedEntity.ReasonForCancel = request.ReasonForCancel;
            var result = await _unitOfWork.ReservationRepository.UpdateAsync(updatedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);

            return new Response<ReservationDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

