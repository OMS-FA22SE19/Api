using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Food : BaseAuditableEntity
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

        public IList<FoodCategory> FoodCategories { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
        public IList<MenuFood> MenuFoods { get; set; }
    }
}
