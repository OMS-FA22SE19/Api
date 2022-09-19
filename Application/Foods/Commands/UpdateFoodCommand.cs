using Application.Categories.Response;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Foods.Commands
{
    public sealed class UpdateFoodCommand : IMapFrom<Food>, IRequest<Response<FoodDto>>
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 5)]
        public string Description { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        [StringLength(2048, MinimumLength = 5)]
        public string PictureUrl { get; set; }

        public IList<CategoryDto>? Categories { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateFoodCommand, Food>()
                .ForSourceMember(dto => dto.Categories, opt => opt.DoNotValidate());
        }
    }

    public sealed class UpdateFoodCommandHandler : IRequestHandler<UpdateFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateFoodCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<FoodDto>> Handle(UpdateFoodCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Food), request.Id);
            }
            var updatedEntity = _mapper.Map<Food>(request);

            if (entity.FoodCategories is not null)
            {
                await _unitOfWork.FoodCategoryRepository.DeleteAsync(entity.FoodCategories);
            }

            if (request.Categories != null && request.Categories.Any())
            {
                foreach (var category in request.Categories)
                {
                    var inDatabase = await _unitOfWork.CategoryRepository.GetAsync(e => e.Id == category.Id);
                    if (inDatabase is null)
                    {
                        throw new NotFoundException(nameof(Category), category.Id);
                    }
                    updatedEntity.FoodCategories.Add(new FoodCategory
                    {
                        Food = updatedEntity,
                        Category = inDatabase
                    });
                }

            }
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
