using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
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
    public sealed class ChangeReservationStatusCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public ReservationStatus Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ChangeReservationStatusCommand, Reservation>();
        }
    }

    public sealed class ChangeReservationStatusCommandHandler : IRequestHandler<ChangeReservationStatusCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public ChangeReservationStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<ReservationDto>> Handle(ChangeReservationStatusCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Reservation), request.Id);
            }

            MapToEntity(request, entity);

            if (request.Status == ReservationStatus.CheckIn)
            {
                if (_dateTime.Now >= entity.StartTime.AddMinutes(-15))
                {
                    var reservationTable = await _unitOfWork.ReservationTableRepository.GetAllAsync();
                    reservationTable.RemoveAll(rt => !(rt.ReservationId == request.Id));
                    foreach (ReservationTable rt in reservationTable)
                    {
                        var table = await _unitOfWork.TableRepository.GetAsync(t => t.Id == rt.TableId);
                        if (table is null)
                        {
                            throw new NotFoundException(nameof(Reservation), rt.TableId);
                        }
                        table.Status = TableStatus.Occupied;
                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }
                } 
                else
                {
                    return new Response<ReservationDto>("It not the time to check in yet");
                }
            }

            var result = await _unitOfWork.ReservationRepository.UpdateAsync(entity);
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

        private static void MapToEntity(ChangeReservationStatusCommand request, Reservation updatedEntity) => updatedEntity.Status = request.Status;
    }
}
