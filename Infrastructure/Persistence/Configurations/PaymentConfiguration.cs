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

            builder.Property<string>("ObjectId")
                .HasColumnType("nvarchar(450)");

            builder.Property("ObjectType")
                .HasColumnType("int");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property<double>("Amount")
                .HasColumnType("float");

            builder.HasKey(e => e.Id);

            builder.ToTable("Payments");
        }
    }
}
