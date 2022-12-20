using Application.Demo.Responses;
using Application.Models;
using AutoMapper;
using Core.Enums;
using Core.Interfaces;
using MediatR;

namespace Application.Demo.Commands
{
    public sealed class ChangeDemoOrderToDone : IRequest<Response<OrderReservationDemoDto>>
    {
        public List<string> OrderIdsToDone { get; set; }
    }

    public sealed class ChangeDemoOrderToDoneHandler : IRequestHandler<ChangeDemoOrderToDone, Response<OrderReservationDemoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeDemoOrderToDoneHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderReservationDemoDto>> Handle(ChangeDemoOrderToDone request, CancellationToken cancellationToken)
        {
            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = new List<string>()
            };

            foreach (var id in request.OrderIdsToDone)
            {
                var OrderDemoDone = await _unitOfWork.OrderRepository.GetAsync(od => od.Id.Equals(id));
                if (OrderDemoDone is null)
                {
                    dto.Error.Add($"Cannot update {id} because it didnt exist");
                }
                else
                {
                    OrderDemoDone.Status = OrderStatus.Paid;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderDemoDone);
                    var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == OrderDemoDone.ReservationId);
                    reservation.Status = ReservationStatus.Done;
                    await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.updated.Add(id);
                }
            }
            return new Response<OrderReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
