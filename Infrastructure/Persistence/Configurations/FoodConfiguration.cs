using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FoodConfiguration : IEntityTypeConfiguration<Food>
    {
        public void Configure(EntityTypeBuilder<Food> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<bool>("Available")
                .HasColumnType("bit");

            builder.Property<DateTime>("Created")
                .HasColumnType("datetime2");

            builder.Property<string>("CreatedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(4000)
                .HasColumnType("nvarchar(4000)");

            builder.Property<string>("Ingredient")
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("nvarchar(2000)");

            builder.Property<bool>("IsDeleted")
                .HasColumnType("bit");

            builder.Property<DateTime?>("LastModified")
                .HasColumnType("datetime2");

            builder.Property<string>("LastModifiedBy")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");

            builder.Property<string>("PictureUrl")
                .IsRequired()
                .HasMaxLength(2048)
                .HasColumnType("nvarchar(2048)");

            builder.Property<int>("CourseTypeId")
                .HasColumnType("int");

            builder.HasOne(e => e.CourseType)
                .WithMany(e => e.Foods)
                .HasForeignKey(e => e.CourseTypeId);

            builder.HasMany(e => e.OrderDetails)
                .WithOne(e => e.Food)
                .HasForeignKey(e => e.FoodId);

            builder.HasMany(e => e.FoodTypes)
                .WithOne(e => e.Food)
                .HasForeignKey(e => e.FoodId);

            builder.HasMany(e => e.MenuFoods)
                .WithOne(e => e.Food)
                .HasForeignKey(e => e.FoodId);


            builder.HasKey("Id");

            builder.ToTable("Foods");
        }
    }
}
