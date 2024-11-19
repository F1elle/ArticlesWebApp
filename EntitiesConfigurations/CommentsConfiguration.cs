using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.EntitiesConfigurations;

public class CommentsConfiguration : IEntityTypeConfiguration<CommentsEntity>
{
    public void Configure(EntityTypeBuilder<CommentsEntity> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ArticleId).IsRequired();
        builder.Property(c => c.OwnerId).IsRequired();
        builder.Property(c => c.Content).IsRequired().HasMaxLength(500);
        builder.Property(c => c.PublishedDate).IsRequired();
        builder.HasMany(c => c.Likes)
            .WithOne()
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Comments)
            .WithOne()
            .HasForeignKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}