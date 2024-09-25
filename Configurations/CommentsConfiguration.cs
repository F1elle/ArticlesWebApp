using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.Configurations;

public class CommentsConfiguration : IEntityTypeConfiguration<CommentsEntity>
{
    public void Configure(EntityTypeBuilder<CommentsEntity> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new {c.ArticleId, c.OwnerId}).IsUnique();
        builder.Property(c => c.Content).IsRequired().HasMaxLength(250);
        builder.Property(c => c.PublishedDate).IsRequired();
    }
}