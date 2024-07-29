using Microsoft.EntityFrameworkCore;
using Order.API.OrderServices;

namespace Order.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
        
    }

    public DbSet<OrderServices.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}