using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"), 1L, 1);

            builder.Property<int>("FoodId")
                .HasColumnType("int");

            builder.Property<string>("OrderId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            builder.Property<double>("Price")
                .HasColumnType("float");

            builder.Property<int>("Quantity")
                .HasColumnType("int");

            builder.Property(e => e.Status)
                .HasColumnType("int");

            builder.HasOne(e => e.Order)
                .WithMany(e => e.OrderDetails)
                .HasForeignKey(e => e.OrderId);

            builder.HasOne(e => e.Food)
                .WithMany(e => e.OrderDetails)
                .HasForeignKey(e => e.FoodId);

            builder.HasKey("Id");

            builder.HasIndex("FoodId");

            builder.HasIndex("OrderId");

            builder.ToTable("OrderDetails");
        }
    }
}