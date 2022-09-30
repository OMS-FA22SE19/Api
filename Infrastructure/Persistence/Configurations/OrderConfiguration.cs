using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

            builder.Property<DateTime>("Created")
                .HasColumnType("datetime2");

            builder.Property<string>("CreatedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<DateTime>("Date")
                .HasColumnType("datetime2");

            builder.Property<bool>("IsDeleted")
                .HasColumnType("bit");

            builder.Property<DateTime?>("LastModified")
                .HasColumnType("datetime2");

            builder.Property<string>("LastModifiedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<double>("PrePaid")
                .HasColumnType("float");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(300)");

            builder.Property<int>("TableId")
                .IsRequired()
                .HasColumnType("int");

            builder.HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Table)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.TableId);

            builder.HasMany(e => e.OrderDetails)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId);

            builder.HasKey("Id");

            builder.HasIndex("UserId");

            builder.ToTable("Orders");
        }
    }
}
