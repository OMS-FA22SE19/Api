using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<DateTime>("Created")
                .HasColumnType("datetime2");

            builder.Property<string>("CreatedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<bool>("IsDeleted")
                .HasColumnType("bit");

            builder.Property<DateTime?>("LastModified")
                .HasColumnType("datetime2");

            builder.Property<string>("LastModifiedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<int>("NumOfSeats")
                .HasColumnType("int");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.Property(e => e.Type)
                .HasColumnType("int");

            builder.HasMany(e => e.Reservations)
                .WithOne(e => e.Table)
                .HasForeignKey(e => e.TableId);

            builder.HasKey("Id");

            builder.ToTable("Tables");
        }
    }
}