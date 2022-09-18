using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime Created { get; set; }
        [MaxLength(300)]
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        [MaxLength(300)]
        public string? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}