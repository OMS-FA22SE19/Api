using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserTopicConfiguration : IEntityTypeConfiguration<UserTopic>
    {
        public void Configure(EntityTypeBuilder<UserTopic> builder)
        {
            builder.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(300)");

            builder.Property<int>("TopicId")
                .HasColumnType("int");

            builder.HasKey(e => new { e.UserId, e.TopicId });

            builder.HasOne(e => e.User)
                .WithMany(e => e.UserTopics)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Topic)
                .WithMany(e => e.UserTopics)
                .HasForeignKey(e => e.TopicId);

            builder.HasIndex("UserId");

            builder.HasIndex("TopicId");

            builder.ToTable("UserTopic");
        }
    }
}