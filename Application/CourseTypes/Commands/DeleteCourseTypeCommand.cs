using Application.Common.Exceptions;
using Application.CourseTypes.Events;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CourseTypes.Commands
{
    public sealed class DeleteCourseTypeCommand : IRequest<Response<CourseTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteCourseTypeCommandHandler : IRequestHandler<DeleteCourseTypeCommand, Response<CourseTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteCourseTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Response<CourseTypeDto>> Handle(DeleteCourseTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.CourseTypeRepository.DeleteAsync(e => e.Id == request.Id);
            await _mediator.Publish(new CourseTypeDeleteEvent()
            {
                id = request.Id,
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<CourseTypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
