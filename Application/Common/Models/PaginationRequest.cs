using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models
{
    public abstract class PaginationRequest
    {
        [Range(1, int.MaxValue)]
        public int PageIndex { get; init; } = 1;
        [Range(1, int.MaxValue)]
        public int PageSize { get; init; } = 100;
        public string? SearchValue { get; init; }
        public bool IsDescending { get; init; }
    }
}
