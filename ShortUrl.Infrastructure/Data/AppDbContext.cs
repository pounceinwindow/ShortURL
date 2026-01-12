using Microsoft.EntityFrameworkCore;
using URLShortener.Entities;

namespace URLShortener.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Click> Clicks { get; set; }
    public DbSet<ShortLink> ShortLinks { get; set; }
}