namespace InSelfLove.Services.Data.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using InSelfLove.Services.Data.CloudinaryServices;
    using CloudinaryDotNet;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class CloudinaryServiceTests
    {
        private CloudinaryService cloudinaryService;

        public CloudinaryServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(@"appsettings.test.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            Account cloudinaryCredentials = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]);

            Cloudinary cloudinaryUtility = new Cloudinary(cloudinaryCredentials);
            this.cloudinaryService = new CloudinaryService(cloudinaryUtility);
        }

        [Fact]
        public async Task UploadImageWorksCorrectly()
        {
            var path = Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "img\\test.jpg");

            using (var stream = File.OpenRead(path))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpg",
                };

                var imageUrl = await this.cloudinaryService.UploadPicture(file, Path.GetFileName(stream.Name));
                Assert.NotNull(imageUrl);

                var urlArray = imageUrl.Split('/');
                var folder = urlArray[urlArray.Length - 2];
                var publicId = urlArray[urlArray.Length - 1].Split('.')[0];
                var deletionResult = await this.cloudinaryService.Delete($"{folder}/{publicId}");
                Assert.Equal("ok", deletionResult);
            }
        }
    }
}
