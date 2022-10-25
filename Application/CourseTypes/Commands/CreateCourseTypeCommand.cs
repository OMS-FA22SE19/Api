using Application.Common.Mappings;
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
    public sealed class CreateCourseTypeCommand : IMapFrom<Core.Entities.CourseType>, IRequest<Response<CourseTypeDto>>
    {
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateCourseTypeCommand, CourseType>();
        }
    }

    public sealed class CreateCourseTypeCommandHandler : IRequestHandler<CreateCourseTypeCommand, Response<CourseTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCourseTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CourseTypeDto>> Handle(CreateCourseTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<CourseType>(request);
            var result = await _unitOfWork.CourseTypeRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<CourseTypeDto>("error");
            }
            var mappedResult = _mapper.Map<CourseTypeDto>(result);
            return new Response<CourseTypeDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
