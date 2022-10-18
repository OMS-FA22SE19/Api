using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.CourseTypes.Commands
{
    public sealed class UpdateCourseTypeCommand : IMapFrom<CourseType>, IRequest<Response<CourseTypeDto>>
    {
        [Required]
        public int Id { get; init; }
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateCourseTypeCommand, CourseType>();
        }
    }

    public sealed class UpdateCourseTypeCommandHandler : IRequestHandler<UpdateCourseTypeCommand, Response<CourseTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCourseTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CourseTypeDto>> Handle(UpdateCourseTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted);
            if (entity is null)
            {
                throw new NotFoundException(nameof(CourseType), request.Id);
            }

            MapToEntity(request, entity);

            var result = await _unitOfWork.CourseTypeRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<CourseTypeDto>("error");
            }
            var mappedResult = _mapper.Map<CourseTypeDto>(result);
            return new Response<CourseTypeDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private static void MapToEntity(UpdateCourseTypeCommand request, CourseType entity) => entity.Name = request.Name;
    }
}
