using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EFCore
{
    public class UcenterDbContext : DbContext
    {
        public UcenterDbContext()
        {
        }
        public UcenterDbContext(DbContextOptions options) : base(options)
        {


        }
        public DbSet<User> Users { get; set; }
    }
}
