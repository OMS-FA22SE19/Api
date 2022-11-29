namespace Core.Interfaces
{
    public interface IBaseAuditableEntity
    {
        DateTime Created { get; set; }
        string? CreatedBy { get; set; }
        bool IsDeleted { get; set; }
        DateTime? LastModified { get; set; }
        string? LastModifiedBy { get; set; }
    }
}