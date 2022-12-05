using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AdminSettingConfiguration : IEntityTypeConfiguration<AdminSetting>
    {
        public void Configure(EntityTypeBuilder<AdminSetting> builder)
        {
            builder.Property<string>("Name")
                .HasColumnType("nvarchar(450)");

            builder.Property<string>("Value")
                .HasColumnType("nvarchar(450)");

            builder.Property<int>("Order")
                .HasColumnType("int");

            builder.HasKey(e => e.Name);

            builder.ToTable("AdminSettings");
        }
    }
}
