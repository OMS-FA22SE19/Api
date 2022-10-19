using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class MenuFood : Entity
    {
        [Required]
        public int MenuId { get; set; }
        [Required]
        public int FoodId { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double Price { get; set; }

        public Menu Menu { get; set; }
        public Food Food { get; set; }
    }
}
