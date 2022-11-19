using Application.Common.Exceptions;
using Application.Common.Models;
using Application.OrderDetails.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.OrderDetails.Queries
{
    public sealed class GetOrderDetailWithIdQuery : IRequest<Response<DishDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetOrderDetailWithIdQueryHandler : IRequestHandler<GetOrderDetailWithIdQuery, Response<DishDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderDetailWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<DishDto>> Handle(GetOrderDetailWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.OrderDetailRepository.GetAsync(e => e.Id == request.Id, $"{nameof(OrderDetail.Food)},{nameof(OrderDetail.Order)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(OrderDetail), request.Id);
            }
            var mappedResult = _mapper.Map<DishDto>(result);
            return new Response<DishDto>(mappedResult);
        }
    }
}
