using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Tables.Queries
{
    public class GetTableWithIdQuery : IRequest<Response<TableDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public class GetTableWithIdQueryHandler : IRequestHandler<GetTableWithIdQuery, Response<TableDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTableWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableDto>> Handle(GetTableWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TableRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted, $"{nameof(Table.TableType)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Table), $"with {request.Id}");
            }
            var mappedResult = _mapper.Map<TableDto>(result);
            return new Response<TableDto>(mappedResult);
        }
    }
}
