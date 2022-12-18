using Application.Common.Exceptions;
using Application.Demo.Responses;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;

namespace Application.Demo.Commands
{
    public sealed class ChangeReservationStatusDemo : IRequest<Response<ReservationDemoDto>>
    {
        public List<int>? ReservationIdsToCancelled { get; set; }
        public List<int>? ReservationIdsToCheckin { get; set; }
        public List<int>? ReservationIdsToReserved { get; set; }
    }

    public sealed class ChangeReservationStatusDemoHandler : IRequestHandler<ChangeReservationStatusDemo, Response<ReservationDemoDto>>
    {
        static Random rnd = new Random();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeReservationStatusDemoHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDemoDto>> Handle(ChangeReservationStatusDemo request, CancellationToken cancellationToken)
        {

            ReservationDemoDto dto = new ReservationDemoDto()
            {
                ReservationCheckIn = new List<int>(),
                ReservationReserved = new List<int>(),
                ReservationCancelled = new List<int>(),
                Error = new List<string>()
            };

            //Cancelled
            foreach (var id in request.ReservationIdsToCancelled)
            {
                var ReservationDemoForCancelled = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id.Equals(id));
                if (ReservationDemoForCancelled is null)
                {
                    dto.Error.Add($"Cannot update {id} because it does not exist or not available");
                }
                else
                {
                    ReservationDemoForCancelled.Status = ReservationStatus.Cancelled;
                    ReservationDemoForCancelled.ReasonForCancel = "Demo";
                    await _unitOfWork.ReservationRepository.UpdateAsync(ReservationDemoForCancelled);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.ReservationCancelled.Add(id);
                }
            }
            
            //Checkin
            var tables = await _unitOfWork.TableRepository.GetAllAsync();
            var tableTypes = await _unitOfWork.TableTypeRepository.GetAllAsync();
            var tableForCheckIn = tables.ToList();
            foreach (var id in request.ReservationIdsToCheckin)
            {
                var ReservationDemoForCheckin = await _unitOfWork.ReservationRepository.GetAsync(r => r.Status != ReservationStatus.Cancelled && r.Id.Equals(id));
                if (ReservationDemoForCheckin is null)
                {
                    dto.Error.Add($"Cannot update {id} because it does not exist or not available");
                }
                else
                {
                    ReservationDemoForCheckin.Status = ReservationStatus.CheckIn;
                    //await _unitOfWork.ReservationRepository.UpdateAsync(ReservationDemoForCheckin);

                    var tableWithCondition = tableForCheckIn.Where(t => t.NumOfSeats == ReservationDemoForCheckin.NumOfSeats && t.TableTypeId == ReservationDemoForCheckin.TableTypeId).ToList();
                    var table = tableWithCondition[rnd.Next(tableWithCondition.Count())];
                    ReservationDemoForCheckin.ReservationTables = new List<ReservationTable>();
                    ReservationDemoForCheckin.ReservationTables.Add(new ReservationTable()
                    {
                        TableId = table.Id,
                    });

                    var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == table.TableTypeId && !e.IsDeleted);
                    if (tableType is null)
                    {
                        throw new NotFoundException(nameof(TableType), table.TableTypeId);
                    }

                    var result = await _unitOfWork.ReservationRepository.UpdateAsync(ReservationDemoForCheckin);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    if (result is null)
                    {
                        return new Response<ReservationDemoDto>("error");
                    }

                    var billing = new Billing
                    {
                        Id = "Demo-" + result.Id,
                        ReservationAmount = table.NumOfSeats * tableType.ChargePerSeat,
                        ReservationId = result.Id
                    };
                    await _unitOfWork.BillingRepository.InsertAsync(billing);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.ReservationCheckIn.Add(id);
                    tableForCheckIn.Remove(table);
                }
            }

            //Reserved
            foreach (var id in request.ReservationIdsToReserved)
            {
                var ReservationDemoForReserved = await _unitOfWork.ReservationRepository.GetAsync(r => r.Status == ReservationStatus.Available && r.Id.Equals(id));
                if (ReservationDemoForReserved is null)
                {
                    dto.Error.Add($"Cannot update {id} because it does not exist or not available");
                }
                else
                {
                    ReservationDemoForReserved.Status = ReservationStatus.Reserved;
                    await _unitOfWork.ReservationRepository.UpdateAsync(ReservationDemoForReserved);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.ReservationReserved.Add(id);
                }
            }

            return new Response<ReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private void MapToEntity(ChangeReservationStatusDemo request, Reservation entity) => entity.Status = request.Status;
    }
}
