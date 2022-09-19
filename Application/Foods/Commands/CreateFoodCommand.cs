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
    public class CreateFoodCommand : IMapFrom<Food>, IRequest<Response<FoodDto>>
    {
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
            profile.CreateMap<CreateFoodCommand, Food>()
                .ForSourceMember(dto => dto.Categories, opt => opt.DoNotValidate());
        }
    }

    public class CreateFoodCommandHandler : IRequestHandler<CreateFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateFoodCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<FoodDto>> Handle(CreateFoodCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Food>(request);
            if (request.Categories != null && request.Categories.Any())
            {
                foreach (var category in request.Categories)
                {
                    var inDatabase = await _unitOfWork.CategoryRepository.GetAsync(e => e.Id == category.Id);
                    if (inDatabase is null)
                    {
                        throw new NotFoundException(nameof(category), category.Id);
                    }
                    entity.FoodCategories.Add(new FoodCategory
                    {
                        Food = entity,
                        Category = inDatabase
                    });
                }

            }
            var result = await _unitOfWork.FoodRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<FoodDto>("error");
            }
            var mappedResult = _mapper.Map<FoodDto>(result);
            return new Response<FoodDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
