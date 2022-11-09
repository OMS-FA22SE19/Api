using Application.Common.Exceptions;
using Application.CourseTypes.Response;
using Application.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CourseTypes.Commands
{
    public sealed class RecoverCourseTypeCommand : IRequest<Response<CourseTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverCourseTypeCommandHandler : IRequestHandler<RecoverCourseTypeCommand, Response<CourseTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverCourseTypeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CourseTypeDto>> Handle(RecoverCourseTypeCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(CourseType), request.Id);
            }
            await _unitOfWork.CourseTypeRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<CourseTypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
