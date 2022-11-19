using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Topics.Commands
{
    public sealed class UpdateTopicCommand : IMapFrom<Topic>, IRequest<Response<TopicDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateTopicCommand, Topic>();
        }
    }

    public sealed class UpdateTopicCommandHandler : IRequestHandler<UpdateTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TopicDto>> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.TopicRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Topic), request.Id);
            }

            MapToEntity(request, entity);

            var result = await _unitOfWork.TopicRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TopicDto>("error");
            }
            var mappedResult = _mapper.Map<TopicDto>(result);

            return new Response<TopicDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private static void MapToEntity(UpdateTopicCommand request, Topic? entity)
        {
            entity.Name = request.Name;
        }
    }
}
