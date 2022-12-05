using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class FoodType : Entity, IEquatable<FoodType>
    {
        [Required]
        public int FoodId { get; set; }
        [Required]
        public int TypeId { get; set; }

        public Food Food { get; set; }
        public Type Type { get; set; }

        public bool Equals(FoodType? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.FoodId == other.FoodId
                && this.TypeId == other.TypeId;
        }
    }

    public sealed class FoodTypeComparer : IEqualityComparer<FoodType>
    {
        public bool Equals(FoodType x, FoodType y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.FoodId == y.FoodId
                && x.TypeId == y.TypeId;
        }

        public int GetHashCode(FoodType FoodType)
        {
            if (Object.ReferenceEquals(FoodType, null)) return 0;

            int hashFoodTypeFoodId = FoodType.FoodId.GetHashCode();

            int hashFoodTypeTypeId = FoodType.TypeId.GetHashCode();


            return hashFoodTypeFoodId ^ hashFoodTypeTypeId;
        }
    }
}
