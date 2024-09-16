using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.Configurations;

public class ArticlesConfiguration : IEntityTypeConfiguration<ArticlesEntity>
{
    public void Configure(EntityTypeBuilder<ArticlesEntity> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(120);
        builder.Property(a => a.Content).IsRequired().HasMaxLength(10000);
        builder.Property(a => a.PublishDate).IsRequired();
        builder.Property(a => a.AuthorId).IsRequired();
        builder.HasMany(a => a.Comments).WithOne().HasForeignKey(c => c.ArticleId);
        builder.HasMany(a => a.Likes).WithOne().HasForeignKey(l => l.ArticleId);
    }
}