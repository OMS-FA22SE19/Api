using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReservationTableConfiguration : IEntityTypeConfiguration<ReservationTable>
    {
        public void Configure(EntityTypeBuilder<ReservationTable> builder)
        {
            builder.Property<int>("ReservationId")
                .HasColumnType("int");

            builder.Property<int>("TableId")
                .HasColumnType("int");

            builder.HasKey(e => new { e.ReservationId, e.TableId });

            builder.HasOne(e => e.Reservation)
                .WithMany(e => e.ReservationTables)
                .HasForeignKey(e => e.ReservationId);

            builder.HasOne(e => e.Table)
                .WithMany(e => e.ReservationsTables)
                .HasForeignKey(e => e.TableId);

            builder.HasIndex("ReservationId");

            builder.HasIndex("TableId");

            builder.ToTable("ReservationTables");
        }
    }
}