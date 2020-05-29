namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Data.Common;

    using BDInSelfLove.Data;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class SqliteSetup : IDisposable
    {
        public DbContextOptions<ApplicationDbContext> ContextOptions { get; set; }

        private DbConnection Connection { get; set; }

        public void Dispose() => this.Connection?.Dispose();

        public void SetupSqlite()
        {
            this.ContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseSqlite(CreateInMemoryDatabase())
              .Options;

            this.Connection = RelationalOptionsExtension.Extract(this.ContextOptions).Connection;
            this.PrepareDatabase();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        private void PrepareDatabase()
        {
            using var context = new ApplicationDbContext(this.ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
