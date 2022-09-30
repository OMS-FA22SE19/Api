using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TableTypeConfiguration : IEntityTypeConfiguration<TableType>
    {
        public void Configure(EntityTypeBuilder<TableType> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.HasMany(e => e.Tables)
                .WithOne(e => e.TableType)
                .HasForeignKey(e => e.TableTypeId);

            builder.HasKey("Id");

            builder.ToTable("TableTypes");
        }
    }
}
