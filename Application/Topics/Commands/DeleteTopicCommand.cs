using Application.Common.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Topics.Commands
{
    public sealed class DeleteTopicCommand : IRequest<Response<TopicDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteTopicCommandHandler : IRequestHandler<DeleteTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TopicDto>> Handle(DeleteTopicCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.TopicRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<TopicDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

