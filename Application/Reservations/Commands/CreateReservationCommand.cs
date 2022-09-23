using Application.Categories.Response;
using Application.Common.Mappings;
using Application.Models;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Commands
{
    public class CreateReservationCommand : IMapFrom<Reservation>, IRequest<Response<ReservationDto>>
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 5)]
        public string Description { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        [StringLength(2048, MinimumLength = 5)]
        public string PictureUrl { get; set; }

        public IList<CategoryDto>? Categories { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateReservationCommand, Reservation>()
                .ForSourceMember(dto => dto.Categories, opt => opt.DoNotValidate());
        }
    }

    public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
