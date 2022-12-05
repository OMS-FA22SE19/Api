using Application.Common.Mappings;
using Application.CourseTypes.Response;
using Application.Menus.Response;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using System.Linq;

namespace Application.Foods.Response
{
    public sealed class FoodDto : IMapFrom<Food>, IEquatable<FoodDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        public string PictureUrl { get; set; }
        public bool IsDeleted { get; set; }
        public CourseTypeDto CourseType { get; set; }
        public IList<TypeDto> Types { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Food, FoodDto>().ReverseMap();
        }

        public bool Equals(FoodDto? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other.CourseType == null)
            {
                if(other.Types == null)
                {
                    return this.Id == other.Id
                        && this.Name == other.Name
                        && this.Description == other.Description
                        && this.Ingredient == other.Ingredient
                        && this.Available == other.Available
                        && this.PictureUrl == other.PictureUrl
                        && this.IsDeleted == other.IsDeleted;
                }
                else
                {
                    return this.Id == other.Id
                        && this.Name == other.Name
                        && this.Description == other.Description
                        && this.Ingredient == other.Ingredient
                        && this.Available == other.Available
                        && this.PictureUrl == other.PictureUrl
                        && this.IsDeleted == other.IsDeleted
                        && this.Types.SequenceEqual(other.Types, new TypeDtoComparer());
                }
            }
            if (other.Types == null)
            {
                return this.Id == other.Id
                    && this.Name == other.Name
                    && this.Description == other.Description
                    && this.Ingredient == other.Ingredient
                    && this.Available == other.Available
                    && this.PictureUrl == other.PictureUrl
                    && this.IsDeleted == other.IsDeleted
                    && this.CourseType.Equals(other.CourseType);
            }
            return this.Id == other.Id
                && this.Name == other.Name
                && this.Description == other.Description
                && this.Ingredient == other.Ingredient
                && this.Available == other.Available
                && this.PictureUrl == other.PictureUrl
                && this.IsDeleted == other.IsDeleted
                && this.CourseType.Equals(other.CourseType)
                && this.Types.SequenceEqual(other.Types, new TypeDtoComparer());
        }
    }

    public sealed class FoodDtoComparer : IEqualityComparer<FoodDto>
    {
        public bool Equals(FoodDto x, FoodDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.Description == y.Description
                && x.Ingredient == y.Ingredient
                && x.Available == y.Available
                && x.PictureUrl == y.PictureUrl
                && x.IsDeleted == y.IsDeleted
                && x.CourseType.Equals(y.CourseType)
                && x.Types.SequenceEqual(y.Types, new TypeDtoComparer());
        }

        public int GetHashCode(FoodDto FoodDto)
        {
            if (Object.ReferenceEquals(FoodDto, null)) return 0;

            int hashFoodDtoName = FoodDto.Name == null ? 0 : FoodDto.Name.GetHashCode();

            int hashFoodDtoDescription = FoodDto.Description == null ? 0 : FoodDto.Description.GetHashCode();

            int hashFoodDtoIngredient = FoodDto.Ingredient == null ? 0 : FoodDto.Ingredient.GetHashCode();

            int hashFoodDtoAvailable = FoodDto.Available == null ? 0 : FoodDto.Available.GetHashCode();

            int hashFoodDtoPictureUrl = FoodDto.PictureUrl == null ? 0 : FoodDto.PictureUrl.GetHashCode();

            int hashFoodDtoIsDeleted = FoodDto.IsDeleted == null ? 0 : FoodDto.IsDeleted.GetHashCode();

            int hashFoodDtoCourseType = FoodDto.CourseType == null ? 0 : FoodDto.CourseType.GetHashCode();

            int hashFoodDtoTypes = FoodDto.Types == null ? 0 : FoodDto.Types.GetHashCode();

            int hashMenuId = FoodDto.Id.GetHashCode();

            return hashFoodDtoName ^ hashFoodDtoDescription ^ hashFoodDtoIngredient ^ hashFoodDtoAvailable ^ hashFoodDtoPictureUrl ^ hashFoodDtoIsDeleted ^ hashFoodDtoCourseType ^ hashFoodDtoTypes ^ hashMenuId;
        }
    }
}
