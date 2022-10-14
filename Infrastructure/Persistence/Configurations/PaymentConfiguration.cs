using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.Property<string>("Id")
                .HasColumnType("nvarchar(450)");

            builder.Property<string>("OrderId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property<double>("Amount")
                .HasColumnType("float");

            builder.HasOne(e => e.Order)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.OrderId);

            builder.HasKey(e => e.Id);

            builder.HasIndex("OrderId");

            builder.ToTable("Payments");
        }
    }
}
