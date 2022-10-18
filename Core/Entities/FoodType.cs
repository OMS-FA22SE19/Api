using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class FoodType : Entity
    {
        [Required]
        public int FoodId { get; set; }
        [Required]
        public int TypeId { get; set; }

        public Food Food { get; set; }
        public Type Type { get; set; }
    }
}
