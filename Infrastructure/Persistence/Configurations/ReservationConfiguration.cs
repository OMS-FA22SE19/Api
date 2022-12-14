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
                .HasColumnType("int");

            builder.Property<int>("TableTypeId")
                .HasColumnType("int");

            builder.Property<int>("NumOfSeats")
                .HasColumnType("int");

            builder.Property<int>("Quantity")
                .HasColumnType("int");

            builder.Property<int>("NumOfEdits")
                .HasColumnType("int");

            builder.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(300)");

            builder.Property<string>("ReasonForCancel")
                .IsRequired(false)
                .HasColumnType("nvarchar(300)");

            builder.Property<string>("FullName")
                .IsRequired()
                .HasColumnType("nvarchar(1000)");

            builder.Property<string>("PhoneNumber")
               .IsRequired()
               .HasColumnType("nvarchar(15)");

            builder.HasOne(e => e.User)
                .WithMany(e => e.Reservations)
                .HasForeignKey(e => e.UserId);

            builder.HasMany(e => e.ReservationTables)
                .WithOne(e => e.Reservation)
                .HasForeignKey(e => e.ReservationId);

            builder.HasKey("Id");

            builder.HasIndex("UserId");

            builder.ToTable("Reservations");
        }
    }
}