using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.EntitiesConfigurations;

public class AuthEventsConfiguration : IEntityTypeConfiguration<AuthEventsEntity>
{
    public void Configure(EntityTypeBuilder<AuthEventsEntity> builder)
    {
        builder.ToTable("AuthEvents");
        builder.HasKey(ae => ae.Id);
        builder.Property(ae => ae.UserId).IsRequired();
        builder.Property(ae => ae.EventType).IsRequired();
        builder.Property(ae => ae.TimeStamp).IsRequired();
        builder.Property(ae => ae.IsSucceeded).IsRequired();
    }
}