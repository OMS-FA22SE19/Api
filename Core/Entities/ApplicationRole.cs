using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class ApplicationRole : IdentityRole
    {
        [MaxLength(50)]
        [Key]
        public override string Id { get; set; }
        public bool IsDeleted { get; set; }
        public virtual List<ApplicationUser> Users { get; set; }
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }
        public ApplicationRole(string roleName) : this()
        {
            Name = roleName;
        }
    }
}