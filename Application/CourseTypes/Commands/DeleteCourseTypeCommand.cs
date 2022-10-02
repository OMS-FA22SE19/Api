using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
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

        public DeleteCourseTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CourseTypeDto>> Handle(DeleteCourseTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.CourseTypeRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<CourseTypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
