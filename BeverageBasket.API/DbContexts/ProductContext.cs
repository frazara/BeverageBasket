using BeverageBasket.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeverageBasket.API.DbContexts
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "ProductDb");
            optionsBuilder.UseInMemoryDatabase(databaseName: "OrderDb");
        }
    }
}
