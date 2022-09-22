using Application.Tables.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace Application.Tables.Queries
{
    public class GetTypeOfTableQuery : IRequest<Response<List<TableTypeDto>>>
    {
        [Required]
        public int NumsOfPeople { get; init; }
    }

    public class GetTypeOfTableQueryHandler : IRequestHandler<GetTypeOfTableQuery, Response<List<TableTypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTypeOfTableQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<TableTypeDto>>> Handle(GetTypeOfTableQuery request, CancellationToken cancellationToken)
        {
            var resultSeatNumber = await _unitOfWork.TableRepository.GetClosestNumOfSeatTable(request.NumsOfPeople);

            var result = await _unitOfWork.TableRepository.GetTableWithSeatsNumber(resultSeatNumber);

            List<TableTypeDto> ListTableType = new List<TableTypeDto>();

            foreach (Table table in result)
            {
                var tableType = ListTableType.SingleOrDefault(t => t.Type == table.Type);
                if (tableType == null)
                {
                    List<int> tableIds = new List<int>();
                    tableIds.Add(table.Id);
                    ListTableType.Add(new TableTypeDto()
                    {
                        Type = table.Type,
                        NumOfSeats = resultSeatNumber,
                        Total = 1,
                        TableIds = tableIds
                    });
                }
                else
                {
                    tableType.Total++;
                    tableType.TableIds.Add(table.Id);
                }
            }
            return new Response<List<TableTypeDto>>(ListTableType);
        }
    }
}
