namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Threading.Tasks;

    public interface ISeeder
    {
        Task SeedAsync(MySqlDbContext dbContext, IServiceProvider serviceProvider);
    }
}
