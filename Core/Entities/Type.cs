using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Type : BaseAuditableEntity
    {
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; set; }

        public IList<FoodType> FoodTypes { get; set; }
    }
}
