using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property<int>("Id")
                .HasColumnType("int");

            builder.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            builder.Property<DateTime>("Created")
                .HasColumnType("datetime2");

            builder.Property<DateTime>("Expired")
                .HasColumnType("datetime2");

            builder.Property<string>("CreatedByIp")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.Property<DateTime?>("Revoked")
                .HasColumnType("datetime2");

            builder.Property<string>("RevokedByIp")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.Property<string>("ReplacedByToken")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.Property<string>("ReasonRevoked")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            //builder.Property<bool>("IsExpired")
            //    .HasColumnType("bit");

            //builder.Property<bool>("IsRevoked")
            //    .HasColumnType("bit");

            ////builder.Property<bool>("IsActive")
            ////    .HasColumnType("bit");

            builder.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasKey(e => e.Id);

            builder.ToTable("RefreshTokens");
        }
    }
}
