using ArticlesWebApp.Api.Entities;
using ArticlesWebApp.Api.EntitiesConfigurations;
using Microsoft.EntityFrameworkCore;

namespace ArticlesWebApp.Api.Data;

public class ArticlesDbContext(
    DbContextOptions<ArticlesDbContext> options,
    IConfiguration configuration)
    : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(configuration.GetConnectionString("DbConnection"));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ArticlesConfiguration());
        modelBuilder.ApplyConfiguration(new RolesConfiguration());
        modelBuilder.ApplyConfiguration(new UsersConfiguration());
        modelBuilder.ApplyConfiguration(new LikesConfiguration());
        modelBuilder.ApplyConfiguration(new CommentsConfiguration());
        modelBuilder.ApplyConfiguration(new AuthEventsConfiguration());
        modelBuilder.ApplyConfiguration(new EventsConfiguration());
    }
    
    public DbSet<UsersEntity> Users { get; set; }
    public DbSet<ArticlesEntity> Articles { get; set; }
    public DbSet<LikesEntity> ArticlesLikes { get; set; }
    public DbSet<LikesEntity> CommentsLikes { get; set; }
    public DbSet<CommentsEntity> Comments { get; set; }
    public DbSet<RolesEntity> Roles { get; set; }
    public DbSet<AuthEventsEntity> AuthEvents { get; set; }
    public DbSet<EventsEntity> Events { get; set; }
}








