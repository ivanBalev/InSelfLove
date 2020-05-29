namespace BDInSelfLove.Services.Data.Tests.Common.Seeders
{
    using BDInSelfLove.Data.Models;

    public static class UserCreator
    {
        public static ApplicationUser[] GetTestUsers()
        {
            return new ApplicationUser[]
            {
                new ApplicationUser { Id = "1", UserName = "User1", },
                new ApplicationUser { Id = "2", UserName = "User2", },
                new ApplicationUser { Id = "3", UserName = "User3", },
            };
        }
    }
}
