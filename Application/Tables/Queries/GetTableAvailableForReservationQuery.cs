using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Tables.Queries
{
    public class GetTableAvailableForReservationQuery : IRequest<Response<TableTypeWithListOfNumOfSeatsDto>>
    {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int NumOfPeople { get; set; }
        [Required]
        public int TableTypeId { get; set; }
        public class GetTableAvailableForReservationQueryHandler : IRequestHandler<GetTableAvailableForReservationQuery, Response<TableTypeWithListOfNumOfSeatsDto>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IDateTime _dateTime;

            public GetTableAvailableForReservationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTime dateTime)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _dateTime = dateTime;
            }

            public async Task<Response<TableTypeWithListOfNumOfSeatsDto>> Handle(GetTableAvailableForReservationQuery request, CancellationToken cancellationToken)
            {
                var result = await _unitOfWork.TableRepository.GetAllAvailableTableWithDateAndTableType(request.StartTime, request.EndTime, request.TableTypeId, request.NumOfPeople);

                if (result is null)
                {
                    throw new NotFoundException("NO AVAILABLE TABLE");
                }

                var typeResult = await _unitOfWork.TableTypeRepository.GetAsync(t => t.Id == request.TableTypeId);

                TableTypeWithListOfNumOfSeatsDto tableType = new TableTypeWithListOfNumOfSeatsDto()
                {
                    TableTypeId = typeResult.Id,
                    TableTypeName = typeResult.Name,
                    numOfPeople = request.NumOfPeople
                };

                List<TableByNumOfSeatDto> listNumOfTableWithSeat = new List<TableByNumOfSeatDto>();
                foreach (Table table in result)
                {
                    var tableNumOfSeat = listNumOfTableWithSeat.FirstOrDefault(t => t.NumOfSeats == table.NumOfSeats);
                    if (tableNumOfSeat == null)
                    {
                        List<int> tableIds = new List<int>();
                        tableIds.Add(table.Id);
                        listNumOfTableWithSeat.Add(new TableByNumOfSeatDto()
                        {
                            NumOfSeats = table.NumOfSeats,
                            Total = 1,
                            TableIds = tableIds
                        });
                    }
                    else
                    {
                        tableNumOfSeat.Total++;
                        tableNumOfSeat.TableIds.Add(table.Id);
                    }
                }

                tableType.ListOfNumOfSeats = listNumOfTableWithSeat;

                var mappedResult = _mapper.Map<List<Table>, List<TableDto>>(result);
                return new Response<TableTypeWithListOfNumOfSeatsDto>(tableType);

            }
        }
    }
}
