using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
    {
        public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
        {
            builder.Property<string>("userId")
                .HasColumnType("nvarchar(450)");
            
            builder.Property<string>("deviceToken")
                .HasColumnType("nvarchar(450)");

            builder.HasOne(e => e.User)
                .WithMany(e => e.UserDeviceTokens)
                .HasForeignKey(e => e.userId);

            builder.HasIndex("userId");

            builder.HasKey("userId");

            builder.ToTable("UserDeviceToken");
        }
    }
}
