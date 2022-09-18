using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Category : BaseAuditableEntity
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; set; }

        public IList<FoodCategory> FoodCategories { get; set; }
    }
}
