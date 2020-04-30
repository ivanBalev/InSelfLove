namespace BDInSelfLove.Web.Tests
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class WebTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> server;

        public WebTests(WebApplicationFactory<Startup> server)
        {
            this.server = server;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Forum")]
        [InlineData("/Home/Appointment")]
        [InlineData("/Home/Contact")]
        [InlineData("/Video/All")]
        [InlineData("/Article/All")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var client = this.server.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task IndexPageShouldHaveFeaturedArticle()
        {
            // At present, this test will work correctly only with the default seeded data.
            var client = this.server.CreateClient();

            var response = await client.GetAsync("/");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"<h5 class=""font-weight-light"">Featured Article</h5>
                        <h1>
                            Test4", responseContent);

            Assert.DoesNotContain(@"<div class=""card - title"">Test4</div>", responseContent);
        }

        [Fact]
        public async Task GetRequestToAppointmentsPageRedirectsToLoginForAnonymousUser()
        {
            // At present, this test will work correctly only with the default seeded data.
            var client = this.server.CreateClient();
            var response = await client.GetAsync("/Home/Appointment");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"Login with Google</button>", responseContent);
        }
    }
}
