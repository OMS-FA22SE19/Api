using Application.Models;
using Application.Tables.Response;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Tables.Queries
{
    public sealed class GetTypeOfTableQuery : IRequest<Response<List<TableByTypeDto>>>
    {
        [Required]
        public int NumsOfPeople { get; init; }
    }

    public sealed class GetTypeOfTableQueryHandler : IRequestHandler<GetTypeOfTableQuery, Response<List<TableByTypeDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTypeOfTableQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<TableByTypeDto>>> Handle(GetTypeOfTableQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TableRepository.GetAllAsync(includeProperties: $"{nameof(Table.TableType)}");

            var listTableType = new List<TableByTypeDto>();

            if (!result.Any())
            {
                return new Response<List<TableByTypeDto>>(listTableType);
            }

            foreach (var table in result)
            {
                var tableType = listTableType.FirstOrDefault(t => t.TableTypeId == table.TableTypeId && t.NumOfSeats == table.NumOfSeats);
                if (tableType != null)
                {
                    continue;
                }

                int quantity = 1;
                bool isValid = false;

                if (table.NumOfSeats >= request.NumsOfPeople)
                {
                    isValid = true;
                }
                else
                {
                    var count = result.Where(e => e.TableTypeId == table.TableTypeId && e.NumOfSeats == table.NumOfSeats && e.TableType.CanBeCombined).Count();
                    quantity = ((request.NumsOfPeople % table.NumOfSeats) == 0) ? request.NumsOfPeople / table.NumOfSeats : (request.NumsOfPeople / table.NumOfSeats) + 1;
                    isValid = count >= quantity && quantity <= 4;
                }

                if (!isValid)
                {
                    continue;
                }

                listTableType.Add(new TableByTypeDto()
                {
                    TableTypeId = table.TableTypeId,
                    TableTypeName = table.TableType.Name,
                    NumOfSeats = table.NumOfSeats,
                    Quantity = quantity
                });
            }
            return new Response<List<TableByTypeDto>>(listTableType.OrderByDescending(e => e.NumOfSeats).ToList());
        }
    }
}
