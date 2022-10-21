using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Type : BaseAuditableEntity, IEquatable<Type>
    {
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; set; }

        public IList<FoodType> FoodTypes { get; set; }

        public bool Equals(Type? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.Name == other.Name;
        }
    }

    public sealed class TypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name;
        }

        public int GetHashCode(Type type)
        {
            if (Object.ReferenceEquals(type, null)) return 0;

            int hashtableTypeName = type.Name == null ? 0 : type.Name.GetHashCode();

            int hashTableId = type.Id.GetHashCode();

            return hashtableTypeName ^ hashTableId;
        }
    }
}
