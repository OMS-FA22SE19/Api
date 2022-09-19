using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class FoodCategory : Entity
    {
        [Required]
        public int FoodId { get; set; }
        [Required]
        public int CategoryId { get; set; }

        public Food Food { get; set; }
        public Category Category { get; set; }
    }
}
