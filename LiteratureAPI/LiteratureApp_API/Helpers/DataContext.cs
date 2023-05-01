namespace LiteratureApp_API.Helpers;
using LiteratureApp_API.Entities;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sql server database
        var a = Configuration.GetConnectionString("WebApiDatabase");
        options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ProfileLiteratureRating> ProfileLiteratureRating { get; set; }
    public DbSet<Literature> Literature { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProfileLiteratureRating>().ToTable("ProfileLiteratureRating");
    }
}