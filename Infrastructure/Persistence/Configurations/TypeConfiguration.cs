using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TypeConfiguration : IEntityTypeConfiguration<Core.Entities.Type>
    {
        public void Configure(EntityTypeBuilder<Core.Entities.Type> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<DateTime>("Created")
                .HasColumnType("datetime2");

            builder.Property<string>("CreatedBy")
                .HasColumnType("nvarchar(300)");

            builder.Property<bool>("IsDeleted")
                .HasColumnType("bit");

            builder.Property<DateTime?>("LastModified")
                .HasColumnType("datetime2");

            builder.Property<string>("LastModifiedBy")
                .HasColumnType("nvarchar(300)");

            builder.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.Property<string>("Description")
                .IsRequired(false)
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.HasMany(e => e.FoodTypes)
                .WithOne(e => e.Type)
                .HasForeignKey(e => e.TypeId);

            builder.HasKey("Id");

            builder.ToTable("Types");
        }
    }
}
