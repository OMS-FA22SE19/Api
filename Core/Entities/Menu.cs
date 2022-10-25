using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Menu : BaseAuditableEntity, IEquatable<Menu>
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength(1000, MinimumLength = 2)]
        public string Description { get; set; }
        public bool IsHidden { get; set; } = true;
        public IList<MenuFood> MenuFoods { get; set; }

        public bool Equals(Menu? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.Name == other.Name
                && this.Description == other.Description
                && this.IsHidden == other.IsHidden;
        }
    }

    public sealed class MenuComparer : IEqualityComparer<Menu>
    {
        public bool Equals(Menu x, Menu y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.Description == y.Description
                && x.IsHidden == y.IsHidden
                && x.IsDeleted == y.IsDeleted;
        }

        public int GetHashCode(Menu menu)
        {
            if (Object.ReferenceEquals(menu, null)) return 0;

            int hashMenuDtoName = menu.Name == null ? 0 : menu.Name.GetHashCode();

            int hashMenuDtoDescription = menu.Description == null ? 0 : menu.Description.GetHashCode();

            int hashMenuDtoIsHidden = menu.IsHidden == null ? 0 : menu.IsHidden.GetHashCode();

            int hashMenuId = menu.Id.GetHashCode();

            return hashMenuDtoName ^ hashMenuDtoDescription ^ hashMenuDtoIsHidden ^ hashMenuId;
        }
    }
}
