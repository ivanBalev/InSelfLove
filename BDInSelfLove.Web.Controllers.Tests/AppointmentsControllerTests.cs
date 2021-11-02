namespace BDInSelfLove.Web.Controllers.Tests
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class AppointmentsControllerTests
    {
        [Fact]
        public async Task Test()
        {
            var serverFactory = new WebApplicationFactory<Startup>();
            var client = serverFactory.CreateClient();

            var response = await client.GetAsync("/");
            var responseAsString = await response.Content.ReadAsStringAsync();

            Assert.Contains("<h1", responseAsString);
        }
    }
}
