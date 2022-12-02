using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Food : BaseAuditableEntity, IEquatable<Food>
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
        [StringLength(2048, MinimumLength = 2)]
        public string PictureUrl { get; set; }
        public int CourseTypeId { get; set; }

        public CourseType CourseType { get; set; }
        public IList<FoodType> FoodTypes { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
        public IList<MenuFood> MenuFoods { get; set; }

        public bool Equals(Food? other)
        {
            if (other.CourseType == null)
            {
                if (other.FoodTypes == null)
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
                        && this.FoodTypes.SequenceEqual(other.FoodTypes, new FoodTypeComparer());
                }
            }
            if (other.FoodTypes == null)
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
                && this.FoodTypes.SequenceEqual(other.FoodTypes, new FoodTypeComparer());
        }
    }

    public sealed class FoodComparer : IEqualityComparer<Food>
    {
        public bool Equals(Food x, Food y)
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
                && x.FoodTypes.SequenceEqual(y.FoodTypes, new FoodTypeComparer());
        }

        public int GetHashCode(Food Food)
        {
            if (Object.ReferenceEquals(Food, null)) return 0;

            int hashFoodName = Food.Name == null ? 0 : Food.Name.GetHashCode();

            int hashFoodDescription = Food.Description == null ? 0 : Food.Description.GetHashCode();

            int hashFoodIngredient = Food.Ingredient == null ? 0 : Food.Ingredient.GetHashCode();

            int hashFoodAvailable = Food.Available == null ? 0 : Food.Available.GetHashCode();

            int hashFoodPictureUrl = Food.PictureUrl == null ? 0 : Food.PictureUrl.GetHashCode();

            int hashFoodIsDeleted = Food.IsDeleted == null ? 0 : Food.IsDeleted.GetHashCode();

            int hashFoodCourseType = Food.CourseType == null ? 0 : Food.CourseType.GetHashCode();

            int hashFoodTypes = Food.FoodTypes == null ? 0 : Food.FoodTypes.GetHashCode();

            int hashMenuId = Food.Id.GetHashCode();

            return hashFoodName ^ hashFoodDescription ^ hashFoodIngredient ^ hashFoodAvailable ^ hashFoodPictureUrl ^ hashFoodIsDeleted ^ hashFoodCourseType ^ hashFoodTypes ^ hashMenuId;
        }
    }
}
