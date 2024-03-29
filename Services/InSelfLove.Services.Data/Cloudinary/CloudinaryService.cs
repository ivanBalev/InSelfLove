﻿namespace InSelfLove.Services.Data.CloudinaryServices
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
            // Get picture bytes
            byte[] destinationData;
            using (var ms = new MemoryStream())
            {
                await pictureFile.CopyToAsync(ms);
                destinationData = ms.ToArray();
            }

            // Upload to Cloudinary
            UploadResult uploadResult = null;
            using (var ms = new MemoryStream(destinationData))
            {
                ImageUploadParams uploadParams = new ImageUploadParams
                {
                    Folder = "article_images",
                    File = new FileDescription(fileName, ms),
                    UseFilename = true,
                    UniqueFilename = false,
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
