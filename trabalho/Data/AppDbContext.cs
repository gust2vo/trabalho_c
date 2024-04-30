using Products.Model;
using Microsoft.EntityFrameworkCore;

namespace Products.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> products { get; set; }
       
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("DataSource=product.sqlite;Cache=Shared");
    }
}
