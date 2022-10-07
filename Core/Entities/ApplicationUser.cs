using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class ApplicationUser : IdentityUser
    {
        [MaxLength(300)]
        public override string Id { get; set; }
        [MaxLength(1000)]
        public string? FullName { get; set; }
        [MaxLength(15)]
        public override string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public IList<Reservation> Reservations { get; set; }
        public IList<Order> Orders { get; set; }
        public string RoleId { get; set; }
        public ApplicationRole Role { get; set; }
    }
}
