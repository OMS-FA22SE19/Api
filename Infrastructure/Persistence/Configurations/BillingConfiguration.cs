using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BillingConfiguration : IEntityTypeConfiguration<Billing>
    {
        public void Configure(EntityTypeBuilder<Billing> builder)
        {
            builder.Property<string>("Id")
                .HasColumnType("nvarchar(450)");

            builder.Property<int>("ReservationId")
                .IsRequired()
                .HasColumnType("int");

            builder.Property<double>("ReservationAmount")
                .HasColumnType("float");

            builder.Property<string>("ReservationEBillingId")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.Property<string>("OrderId")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.Property<double>("OrderAmount")
                .HasColumnType("float");

            builder.Property<string>("OrderEBillingId")
                .HasColumnType("nvarchar(450)").IsRequired(false);

            builder.HasOne(e => e.Reservation)
                .WithOne(e => e.Billing)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Order)
                .WithOne(e => e.Billing)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasKey(e => e.Id);

            builder.ToTable("Billings");
        }
    }
}
