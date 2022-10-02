using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CourseTypes.Queries
{
    public sealed class GetCourseTypeWithIdQuery : IRequest<Response<CourseTypeDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetCourseTypeWithIdQueryHandler : IRequestHandler<GetCourseTypeWithIdQuery, Response<CourseTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCourseTypeWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CourseTypeDto>> Handle(GetCourseTypeWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.Id);
            var mappedResult = _mapper.Map<CourseTypeDto>(result);
            return new Response<CourseTypeDto>(mappedResult);
        }
    }
}
