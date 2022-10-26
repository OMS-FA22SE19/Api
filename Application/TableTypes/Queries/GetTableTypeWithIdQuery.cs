using Application.Common.Exceptions;
using Application.Models;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.TableTypes.Queries
{
    public sealed class GetTableTypeWithIdQuery : IRequest<Response<TableTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetTableTypeWithIdQueryHandler : IRequestHandler<GetTableTypeWithIdQuery, Response<TableTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTableTypeWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TableTypeDto>> Handle(GetTableTypeWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == request.Id);
            if (result is null)
            {
                throw new NotFoundException(nameof(TableType), request.Id);
            }
            var filters = new List<Expression<Func<Table, bool>>>();
            filters.Add(e => e.TableTypeId == request.Id && !e.IsDeleted);
            var tablesInType = await _unitOfWork.TableRepository.GetAllAsync(filters);
            var mappedResult = _mapper.Map<TableTypeDto>(result);
            mappedResult.Quantity = tablesInType.Count;
            return new Response<TableTypeDto>(mappedResult);
        }
    }
}
