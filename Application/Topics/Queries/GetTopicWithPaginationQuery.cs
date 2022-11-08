using Application.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Topics.Queries
{
    public sealed class GetTopicWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<TopicDto>>>
    {
        public TopicProperty? OrderBy { get; set; }
    }

    public sealed class GetTopicWithPaginationQueryHandler : IRequestHandler<GetTopicWithPaginationQuery, Response<PaginatedList<TopicDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTopicWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<TopicDto>>> Handle(GetTopicWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Topic, bool>>> filters = new();
            Func<IQueryable<Topic>, IOrderedQueryable<Topic>> orderBy = null;
            string includeProperties = $"{nameof(Topic.UserTopics)}.{nameof(UserTopic.User)}";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Name.Contains(request.SearchValue)
                    || request.SearchValue.Equals(e.Id.ToString()));
            }

            switch (request.OrderBy)
            {
                case TopicProperty.Id:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Id);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Id);
                    break;
                case TopicProperty.Name:
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.TopicRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Topic>, PaginatedList<TopicDto>>(result);
            return new Response<PaginatedList<TopicDto>>(mappedResult);
        }
    }
}
