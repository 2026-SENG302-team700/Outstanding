using Microsoft.EntityFrameworkCore;
using SENG302Template.Api.Models.Entities;

namespace SENG302Template.Api.DataAccess;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<Book> Books { get; set; }
}