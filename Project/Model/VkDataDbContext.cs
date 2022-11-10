using Microsoft.EntityFrameworkCore;

namespace Project;

public class VkDataDbContext : DbContext
{
    public VkDataDbContext(DbContextOptions<VkDataDbContext> options) : base(options)
    {
    }

    public DbSet<PostData> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("AppDb");
        optionsBuilder.UseNpgsql(connectionString);
    }
}