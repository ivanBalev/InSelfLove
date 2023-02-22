namespace InSelfLove.Data
{
    using System.IO;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
    {
        public MySqlDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            //var builder = new DbContextOptionsBuilder<MySqlDbContext>();
            //var connectionString = configuration.GetConnectionString("MySql");
            //builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new MySqlDbContext(configuration);
        }
    }
}
