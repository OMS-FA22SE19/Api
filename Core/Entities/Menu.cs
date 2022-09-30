using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Menu : BaseAuditableEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength(1000, MinimumLength = 2)]
        public string Description { get; set; }
        public bool IsHidden { get; set; } = true;
        public IList<MenuFood> MenuFoods { get; set; }
    }
}
