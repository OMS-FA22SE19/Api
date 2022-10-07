using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Tables.Commands
{
    public sealed class UpdateTableCommand : IMapFrom<Table>, IRequest<Response<TableDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int NumOfSeats { get; set; }
        [EnumDataType(typeof(TableStatus))]
        [JsonConverter(typeof(StringEnumConverter))]
        public TableStatus Status { get; set; }
        public int TableTypeId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateTableCommand, Table>();
        }
    }

    public sealed class UpdateTableCommandHandler : IRequestHandler<UpdateTableCommand, Response<TableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableDto>> Handle(UpdateTableCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.TableRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Table), request.Id);
            }
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == request.TableTypeId);
            if (tableType is null)
            {
                throw new NotFoundException(nameof(Table.TableType), request.TableTypeId);
            }

            MapToEntity(request, entity);

            var result = await _unitOfWork.TableRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TableDto>("error");
            }
            var mappedResult = _mapper.Map<TableDto>(result);

            return new Response<TableDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private static void MapToEntity(UpdateTableCommand request, Table? entity)
        {
            entity.NumOfSeats = request.NumOfSeats;
            entity.Status = request.Status;
            entity.TableTypeId = request.TableTypeId;
        }
    }
}
