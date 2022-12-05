using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class AdminSetting : Entity
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
    }
}
