using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<DateTime>("StartTime")
                .HasColumnType("datetime2");

            builder.Property<DateTime>("EndTime")
                .HasColumnType("datetime2");

            builder.Property<bool>("IsPriorFoodOrder")
                .HasColumnType("bit");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property<int>("NumOfPeople")
                .HasDefaultValue(1)
                .HasColumnType("int");

            builder.Property<int>("TableId")
                .HasColumnType("int");

            builder.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(300)");

            builder.HasOne(e => e.User)
                .WithMany(e => e.Reservations)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Table)
                .WithMany(e => e.Reservations)
                .HasForeignKey(e => e.TableId);

            builder.HasKey("Id");

            builder.HasIndex("TableId");

            builder.HasIndex("UserId");

            builder.ToTable("Reservations");
        }
    }
}