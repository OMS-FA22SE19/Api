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
    public class CreateTableTypeCommand : IMapFrom<TableType>, IRequest<Response<TableTypeDto>>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double ChargePerSeat { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTableTypeCommand, TableType>();
        }
    }

    public class CreateTableTypeCommandHandler : IRequestHandler<CreateTableTypeCommand, Response<TableTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTableTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableTypeDto>> Handle(CreateTableTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<TableType>(request);
            var result = await _unitOfWork.TableTypeRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TableTypeDto>("error");
            }
            var mappedResult = _mapper.Map<TableTypeDto>(result);
            return new Response<TableTypeDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
