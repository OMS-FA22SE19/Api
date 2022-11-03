using Core.Common;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class AdminSetting : Entity
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
