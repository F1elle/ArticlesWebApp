using ArticlesWebApp.Api.Common;
using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.EntitiesConfigurations;

public class RolesConfiguration : IEntityTypeConfiguration<RolesEntity>
{
    public void Configure(EntityTypeBuilder<RolesEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(r => r.Permissions).IsRequired();
        
        builder.HasMany(r => r.Users).WithOne(u => u.Role);
        
        RolesEntity[] roles = Enum.GetValues<Roles>().Select(r => new RolesEntity
        {
            Id = (int)r,
            Name = r.ToString(),
            Permissions = Enum.GetValues<Permissions>().Where(p => (int)r > (int)p).ToList()
        }).ToArray(); 
        
        builder.HasData(roles);
    }
}


