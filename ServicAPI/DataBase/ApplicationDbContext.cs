using Microsoft.EntityFrameworkCore;

namespace ServicAPI.DataBase
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Models.Model.User> Users { get; set; }
        public DbSet<Models.Model.Product> Products { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DataForAPI.db");
            
        }
    }
}
