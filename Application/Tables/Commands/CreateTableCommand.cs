using Application.Categories.Response;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Tables.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Application.Foods.Response;
using Core.Enums;

namespace Application.Tables.Commands
{
    public class CreateTableCommand : IMapFrom<Table>, IRequest<Response<TableDto>>
    {
        [Required]
        public int NumOfSeats { get; set; }
        [Required]
        public TableType Type { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTableCommand, Table>();
        }
    }

    public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Response<TableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableDto>> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Table>(request);
            entity.Status = TableStatus.Available;
            var result = await _unitOfWork.TableRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TableDto>("error");
            }
            var mappedResult = _mapper.Map<TableDto>(result);
            return new Response<TableDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
