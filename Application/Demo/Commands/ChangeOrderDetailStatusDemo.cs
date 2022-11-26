using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.OrderDetails.Events;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Demo.Commands
{
    public sealed class ChangeOrderDetailStatusDemo : IMapFrom<OrderDetail>, IRequest<Response<DishDto>>
    {
        public int? numOfProcessing { get; set; }
        public int? numOfServed { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ChangeOrderDetailStatusDemo, OrderDetail>();
        }
    }

    public sealed class ChangeOrderDetailStatusDemoHandler : IRequestHandler<ChangeOrderDetailStatusDemo, Response<DishDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeOrderDetailStatusDemoHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<DishDto>> Handle(ChangeOrderDetailStatusDemo request, CancellationToken cancellationToken)
        {
            List<Expression<Func<OrderDetail, bool>>> filters = new();
            filters.Add(od => od.Status == OrderDetailStatus.Received && od.OrderId.Substring(0,5).Equals("demo-"));
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null;
            orderBy = e => e.OrderBy(x => x.CreatedBy);

            var OrderDetailDemo = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters, orderBy);
            for (int i = 0; i < request.numOfProcessing; i++)
            {
                OrderDetailDemo[i].Status= OrderDetailStatus.Processing;
                await _unitOfWork.OrderDetailRepository.UpdateAsync(OrderDetailDemo[i]);
                await _unitOfWork.CompleteAsync(cancellationToken);
            }
            return new Response<DishDto>("success")
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private void MapToEntity(ChangeOrderDetailStatusDemo request, OrderDetail entity) => entity.Status = request.Status;
    }
}
