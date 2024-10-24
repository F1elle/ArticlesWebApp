using ArticlesWebApp.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticlesWebApp.Api.Configurations;

public class EventsConfiguration : IEntityTypeConfiguration<EventsEntity>
{
    public void Configure(EntityTypeBuilder<EventsEntity> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.SubjectId).IsRequired();
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.EventTime).IsRequired();
        builder.Property(e => e.IsSucceeded).IsRequired();
    }
}