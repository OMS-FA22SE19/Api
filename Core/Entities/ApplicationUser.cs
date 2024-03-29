﻿using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class ApplicationUser : IdentityUser, IBaseAuditableEntity
    {
        [MaxLength(300)]
        public override string Id { get; set; }
        [MaxLength(1000)]
        public string? FullName { get; set; }
        [MaxLength(15)]
        public override string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public IList<Reservation> Reservations { get; set; }
        public IList<UserDeviceToken> UserDeviceTokens { get; set; }
        public IList<UserTopic> UserTopics { get; set; }
        public IList<RefreshToken> RefreshTokens { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
