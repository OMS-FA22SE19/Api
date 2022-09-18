using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FoodCategoryConfiguration : IEntityTypeConfiguration<FoodCategory>
    {
        public void Configure(EntityTypeBuilder<FoodCategory> builder)
        {
            builder.Property<int>("CategoryId")
                .HasColumnType("int");

            builder.Property<int>("FoodId")
                .HasColumnType("int");

            builder.HasKey(e => new { e.CategoryId, e.FoodId });

            builder.HasOne(e => e.Category)
                .WithMany(e => e.FoodCategories)
                .HasForeignKey(e => e.CategoryId);

            builder.HasOne(e => e.Food)
                .WithMany(e => e.FoodCategories)
                .HasForeignKey(e => e.FoodId);

            builder.HasIndex("CategoryId");

            builder.HasIndex("FoodId");

            builder.ToTable("FoodCategory");
        }
    }
}
