using Application.Categories.Response;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Reservations.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

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

        public ChangeReservationStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDto>> Handle(ChangeReservationStatusCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Reservation), request.Id);
            }
            var updatedEntity = _mapper.Map<Reservation>(entity);
            updatedEntity.Status = request.Status;
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
