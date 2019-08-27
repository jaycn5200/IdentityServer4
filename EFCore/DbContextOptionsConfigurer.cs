using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCore
{
    public static class DbContextOptionsConfigurer
    {
        public static void Configure(
            DbContextOptionsBuilder<UcenterDbContext> dbContextOptions, 
            string connectionString
            )
        {
            /* This is the single point to configure DbContextOptions for UcenterDbContext */
            dbContextOptions.UseMySql(connectionString);
        }
        public static void Configure(DbContextOptionsBuilder<DbContext> builder, DbConnection connection)
        {
            builder
                .UseMySql(connection);
        }
    }
}
