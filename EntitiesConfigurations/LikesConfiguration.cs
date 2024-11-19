using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.EntitiesConfigurations;

public class LikesConfiguration : IEntityTypeConfiguration<LikesEntity>
{
    public void Configure(EntityTypeBuilder<LikesEntity> builder)
    {
        builder.ToTable("Likes");
        builder.HasKey(l => new { l.PostId, l.OwnerId });
    }
    
}