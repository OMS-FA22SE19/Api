using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Type = Core.Entities.Type;

namespace Application.Foods.Commands
{
    public sealed class UpdateFoodCommand : IMapFrom<Food>, IRequest<Response<FoodDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 2)]
        public string Description { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 2)]
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        [Required]
        public IFormFile Picture { get; set; }
        public int CourseTypeId { get; set; }

        public IList<int> Types { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateFoodCommand, Food>()
                .ForSourceMember(dto => dto.Types, opt => opt.DoNotValidate());
        }
    }

    public sealed class UpdateFoodCommandHandler : IRequestHandler<UpdateFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;

        public UpdateFoodCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUploadService uploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadService = uploadService;
        }

        public async Task<Response<FoodDto>> Handle(UpdateFoodCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Food.FoodTypes)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Food), request.Id);
            }

            var courseType = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.CourseTypeId && !e.IsDeleted);
            if (courseType is null)
            {
                throw new NotFoundException(nameof(Food.CourseType), request.CourseTypeId);
            }
            var updatedEntity = _mapper.Map<Food>(request);

            if (entity.FoodTypes is not null)
            {
                await _unitOfWork.FoodTypeRepository.DeleteAsync(entity.FoodTypes);
            }

            if (request.Types != null && request.Types.Any())
            {
                foreach (var categoryId in request.Types)
                {
                    var inDatabase = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == categoryId);
                    if (inDatabase is null)
                    {
                        throw new NotFoundException(nameof(Type), categoryId);
                    }
                    if (updatedEntity.FoodTypes is null)
                    {
                        updatedEntity.FoodTypes = new List<FoodType>();
                    }
                    updatedEntity.FoodTypes.Add(new FoodType
                    {
                        FoodId = entity.Id,
                        TypeId = inDatabase.Id
                    });
                }

            }

            var pictureUrl = await _uploadService.UploadAsync(request.Picture, "Foods");
            updatedEntity.PictureUrl = pictureUrl;
            var result = await _unitOfWork.FoodRepository.UpdateAsync(updatedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<FoodDto>("error");
            }
            var mappedResult = _mapper.Map<FoodDto>(result);
            return new Response<FoodDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
