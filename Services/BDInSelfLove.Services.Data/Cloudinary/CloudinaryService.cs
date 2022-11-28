namespace BDInSelfLove.Services.Data.CloudinaryServices
{
    using System.Collections.Generic;
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
                // TODO: Implement responsive breakpoints after website layout is finalized.
                // Changes of media using srcset and sizes attributes can be really tricky.
                ImageUploadParams uploadParams = new ImageUploadParams
                {
                    Folder = "article_images",
                    File = new FileDescription(fileName, ms),
                    UseFilename = true,
                    UniqueFilename = false,
                    //EagerTransforms = new List<Transformation>()
                    //{
                    //    new EagerTransformation().AspectRatio(16, 9)
                    //      .Crop("crop")/*.Gravity("face")*/,
                    //    new Transformation().Width(660).Height(400)
                    //      .Crop("pad").Background("blue"),
                    //},
                    //ResponsiveBreakpoints = { },
                };

                uploadResult = this.cloudinaryUtility.Upload(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                return null;
            }

            // TODO: store img url for preview img and data for full article img
            // single quality for preview img.
            return uploadResult?.SecureUrl.AbsoluteUri;
        }

        public async Task<string> Delete(string folderAndPublicId)
        {
            var result = await this.cloudinaryUtility.DestroyAsync(new DeletionParams(folderAndPublicId));
            return result.Result;
        }
    }
}
