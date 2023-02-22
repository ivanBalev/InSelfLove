namespace InSelfLove.Data
{
    using System;
    using System.Data.Common;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class SqliteDbContext : MySqlDbContext
    {
        private readonly IServiceProvider serviceProvider;

        public SqliteDbContext(IConfiguration configuration, IServiceProvider serviceProvider)
            : base(configuration)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connection = this.serviceProvider.GetRequiredService<DbConnection>();
            options.UseSqlite(connection);
        }
    }
}
