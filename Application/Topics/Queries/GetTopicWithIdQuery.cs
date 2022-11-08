using Application.Common.Exceptions;
using Application.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Topics.Queries
{
    public sealed class GetTopicWithIdQuery : IRequest<Response<TopicDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetTopicWithIdQueryHandler : IRequestHandler<GetTopicWithIdQuery, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTopicWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TopicDto>> Handle(GetTopicWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TopicRepository.GetAsync(e => e.Id == request.Id);
            if (result is null)
            {
                throw new NotFoundException(nameof(Topic), request.Id);
            }
            var filters = new List<Expression<Func<Topic, bool>>>();
            filters.Add(e => e.Id == request.Id && !e.IsDeleted);
            var tablesInType = await _unitOfWork.TopicRepository.GetAllAsync(filters, null, $"{nameof(Topic.UserTopics)}.{nameof(UserTopic.User)}");
            var mappedResult = _mapper.Map<TopicDto>(result);
            return new Response<TopicDto>(mappedResult);
        }
    }
}
