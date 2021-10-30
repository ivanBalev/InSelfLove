namespace BDInSelfLove.Services.Data.CloudinaryServices
{
    using System.IO;
    using System.Threading.Tasks;

    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using Microsoft.AspNetCore.Http;

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary cloudinaryUtility;

        public CloudinaryService(Cloudinary cloudinaryUtility)
        {
            this.cloudinaryUtility = cloudinaryUtility;
        }

        public async Task<string> UploadPicture(IFormFile pictureFile, string fileName)
        {
            byte[] destinationData;

            using (var ms = new MemoryStream())
            {
                await pictureFile.CopyToAsync(ms);
                destinationData = ms.ToArray();
            }

            UploadResult uploadResult = null;

            using (var ms = new MemoryStream(destinationData))
            {
                ImageUploadParams uploadParams = new ImageUploadParams
                {
                    Folder = "article_images",
                    File = new FileDescription(fileName, ms),
                };

                uploadResult = this.cloudinaryUtility.Upload(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                return null;
            }

            return uploadResult?.SecureUrl.AbsoluteUri;
        }

        public async Task<string> Delete(string folderAndPublicId)
        {
            var result = await this.cloudinaryUtility.DestroyAsync(new DeletionParams(folderAndPublicId));
            return result.Result;
        }
    }
}
