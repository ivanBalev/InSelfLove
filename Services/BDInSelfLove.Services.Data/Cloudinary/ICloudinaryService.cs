namespace BDInSelfLove.Services.Data.CloudinaryServices
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public interface ICloudinaryService
    {
        public Task<string> UploadPicture(IFormFile pictureFile, string fileName);

        public Task<string> Delete(string folderAndPublicId);
    }
}
