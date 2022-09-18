using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
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

            builder.HasMany(e => e.FoodCategories)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            builder.HasKey("Id");

            builder.ToTable("Categories");
        }
    }
}
