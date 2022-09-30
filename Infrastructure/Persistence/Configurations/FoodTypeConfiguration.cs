using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FoodTypeConfiguration : IEntityTypeConfiguration<FoodType>
    {
        public void Configure(EntityTypeBuilder<FoodType> builder)
        {
            builder.Property<int>("TypeId")
                .HasColumnType("int");

            builder.Property<int>("FoodId")
                .HasColumnType("int");

            builder.HasKey(e => new { e.TypeId, e.FoodId });

            builder.HasOne(e => e.Type)
                .WithMany(e => e.FoodTypes)
                .HasForeignKey(e => e.TypeId);

            builder.HasOne(e => e.Food)
                .WithMany(e => e.FoodTypes)
                .HasForeignKey(e => e.FoodId);

            builder.HasIndex("TypeId");

            builder.HasIndex("FoodId");

            builder.ToTable("FoodType");
        }
    }
}
