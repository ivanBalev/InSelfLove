namespace BDInSelfLove.Services.Data.CloudinaryService
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public interface ICloudinaryService
    {
        public Task<string> UploadPicture(IFormFile pictureFile, string fileName);
    }
}
