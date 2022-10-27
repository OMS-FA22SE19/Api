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

            builder.Property("ReservationId")
                .HasColumnType("int");

            builder.Property<string>("OrderId")
                .HasColumnType("nvarchar(450)");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property<double>("Amount")
                .HasColumnType("float");

            builder.HasOne(e => e.reservation)
                .WithMany(e => e.Payments)
                .HasForeignKey(e => e.ReservationId);

            builder.HasOne(e => e.order)
                .WithOne(e => e.Payment)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasKey(e => e.Id);

            builder.HasIndex("ReservationId");

            builder.HasIndex("OrderId");

            builder.ToTable("Payments");
        }
    }
}
