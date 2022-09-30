using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Tables.Queries
{
    public class GetTypeOfTableQuery : IRequest<Response<List<TableByTypeDto>>>
    {
        [Required]
        public int NumsOfPeople { get; init; }
    }

    public class GetTypeOfTableQueryHandler : IRequestHandler<GetTypeOfTableQuery, Response<List<TableByTypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTypeOfTableQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<TableByTypeDto>>> Handle(GetTypeOfTableQuery request, CancellationToken cancellationToken)
        {
            var resultSeatNumber = await _unitOfWork.TableRepository.GetClosestNumOfSeatTable(request.NumsOfPeople);
            if (resultSeatNumber <= 0)
            {
                return new Response<List<TableByTypeDto>>($"No available table for numOfSeat: {request.NumsOfPeople}");
            }

            var result = await _unitOfWork.TableRepository.GetTableWithSeatsNumber(resultSeatNumber);

            List<TableByTypeDto> ListTableType = new List<TableByTypeDto>();

            foreach (Table table in result)
            {
                var tableType = ListTableType.FirstOrDefault(t => t.TableTypeId == table.TableTypeId);
                if (tableType == null)
                {
                    List<int> tableIds = new List<int>();
                    tableIds.Add(table.Id);
                    ListTableType.Add(new TableByTypeDto()
                    {
                        TableTypeId = table.TableTypeId,
                        TableTypeName = table.TableType.Name,
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
            return new Response<List<TableByTypeDto>>(ListTableType);
        }
    }
}
