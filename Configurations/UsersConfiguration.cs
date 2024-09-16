using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<UsersEntity>
{
    public void Configure(EntityTypeBuilder<UsersEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.Property(u => u.RegisterDate).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasMany(u => u.Articles).WithOne().HasForeignKey(a => a.AuthorId);
        builder.HasMany(u => u.Comments).WithOne().HasForeignKey(c => c.UserId);
        builder.HasMany(u => u.Likes).WithOne().HasForeignKey(l => l.UserId);
    }
    
}