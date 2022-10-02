using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.TableTypes.Commands
{
    public sealed class UpdateTableTypeCommand : IMapFrom<TableType>, IRequest<Response<TableTypeDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double ChargePerSeat { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateTableTypeCommand, TableType>();
        }
    }

    public sealed class UpdateTableTypeCommandHandler : IRequestHandler<UpdateTableTypeCommand, Response<TableTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTableTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableTypeDto>> Handle(UpdateTableTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(TableType), request.Id);
            }

            var updatedEntity = _mapper.Map<TableType>(request);

            var result = await _unitOfWork.TableTypeRepository.UpdateAsync(updatedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TableTypeDto>("error");
            }
            var mappedResult = _mapper.Map<TableTypeDto>(result);

            return new Response<TableTypeDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
