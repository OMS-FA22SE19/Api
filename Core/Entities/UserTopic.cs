using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class UserTopic : Entity
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int TopicId { get; set; }

        public ApplicationUser User { get; set; }
        public Topic Topic { get; set; }
    }
}
