using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Foods.Events;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Foods.Commands
{
    public sealed class CreateFoodCommand : IMapFrom<Food>, IRequest<Response<FoodDto>>
    {
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

        public IList<int>? Types { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateFoodCommand, Food>()
                .ForSourceMember(dto => dto.Types, opt => opt.DoNotValidate());
        }
    }

    public sealed class CreateFoodCommandHandler : IRequestHandler<CreateFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;

        public CreateFoodCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUploadService uploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadService = uploadService;
        }

        public async Task<Response<FoodDto>> Handle(CreateFoodCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Food>(request);

            var courseType = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.CourseTypeId && !e.IsDeleted);
            if (courseType is null)
            {
                throw new NotFoundException(nameof(Food.CourseType), request.CourseTypeId);
            }

            if (request.Types?.Any() == true)
            {
                foreach (var typeId in request.Types)
                {
                    var inDatabase = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == typeId);
                    if (inDatabase is null)
                    {
                        throw new NotFoundException(nameof(Core.Entities.Type), typeId);
                    }
                    if (entity.FoodTypes is null)
                    {
                        entity.FoodTypes = new List<FoodType>();
                    }
                    entity.FoodTypes.Add(new FoodType
                    {
                        Food = entity,
                        TypeId = inDatabase.Id
                    });
                }

            }

            var pictureUrl = await _uploadService.UploadAsync(request.Picture, "Foods");
            entity.PictureUrl = pictureUrl;
            var result = await _unitOfWork.FoodRepository.InsertAsync(entity);
            entity.AddDomainEvent(new CreateFoodEvent()
            {
                Name = request.Name
            });
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
