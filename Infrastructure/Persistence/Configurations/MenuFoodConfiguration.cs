using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MenuFoodConfiguration : IEntityTypeConfiguration<MenuFood>
    {
        public void Configure(EntityTypeBuilder<MenuFood> builder)
        {
            builder.Property<int>("FoodId")
                .HasColumnType("int");

            builder.Property<int>("MenuId")
                .HasColumnType("int");

            builder.Property<double>("Price")
                .HasColumnType("float");

            builder.HasKey(e => new { e.FoodId, e.MenuId });

            builder.HasOne(e => e.Food)
                .WithMany(e => e.MenuFoods)
                .HasForeignKey(e => e.FoodId);

            builder.HasOne(e => e.Menu)
                .WithMany(e => e.MenuFoods)
                .HasForeignKey(e => e.MenuId);

            builder.HasIndex("FoodId");

            builder.HasIndex("MenuId");

            builder.ToTable("MenuFood");
        }
    }
}