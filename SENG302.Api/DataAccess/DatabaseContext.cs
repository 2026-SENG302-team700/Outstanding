using Microsoft.EntityFrameworkCore;
using SENG302.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.DataAccess;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed a default user
        var user = new User
        {
            Id = 1, // make sure this matches your key type
            DisplayName = "admin",
            Email = "admin@outstanding.com",
            Country = "NZ",
            EmailVerified = true
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;
        modelBuilder.Entity<User>().HasData(user);
    }
}